using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using SSC = Microsoft.Surface.Presentation.Controls;

namespace ScatterPuzzle
{
    /// <summary>
    /// Main Window
    /// </summary>
    public partial class Window1 : SurfaceWindow
    {
        #region Private Members

        // The PuzzleManager.
        private readonly PuzzleManager puzzleManager = new PuzzleManager();
        
        // Keep track of the currently selected puzzle.
        private Visual currentPuzzle;

        // Controls whether a new puzzle can be selected.
        private bool selectionEnabled = true;

        // The puzzle image.
        private VisualBrush puzzleBrush;
        
        // Paths where puzzles should be loaded from.
        private readonly string videoPuzzlesPath;
        private readonly string photoPuzzlesPath;

        // Random number generator to create random animations.
        private readonly Random random = new Random();

        // The dimensions to use when loading a puzzle.
        private int rowCount;
        private int colCount;

        // Animation events could step on each other's toes, need something to lock on
        private bool joinInProgress;

        #endregion

        #region Initalization

        //---------------------------------------------------------//
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Window1()
        {
            InitializeComponent();

            //query from shell to find out where sample photos and videos are stored.
            videoPuzzlesPath = CommonFolder.GetVideoPath();
            photoPuzzlesPath = CommonFolder.GetPhotoPath();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            LoadPuzzleList();
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            // Start all movies with audio disabled.
            StartAllMedia();

            // Enable audio on the active puzzle if it is a video.
            if (puzzleBrush != null)
            {
                StartMediaItem(puzzleBrush.Visual, false);
            }
        }

        //---------------------------------------------------------//
        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            // Start all movies with audio disabled.
            StartAllMedia();
        }

        //---------------------------------------------------------//
        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            // Stop all the videos so they don't waste resources while the app isn't even active
            ForEachMediaElement(delegate(MediaElement movie) 
            {
                movie.Clock.Controller.Stop(); 
            });
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Load all the images/movies in the sample images/movies directories into the puzzle list.
        /// </summary>
        private void LoadPuzzleList()
        {
            // Load photo puzzles
            foreach (string file in Directory.GetFiles(photoPuzzlesPath, "*.jpg"))
            {
                // Load the photo
                Image img = new Image();
                img.Source = new BitmapImage(new Uri(file));
                
                // Add the photo to the list of possible photos
                AddElementToPuzzleList(img);
            }

            // Load video puzzles
            foreach (string file in Directory.GetFiles(videoPuzzlesPath, "*.wmv"))
            {
                // Load the video into a looping timeline
                MediaElement video = new MediaElement();
                MediaTimeline t = new MediaTimeline();
                t.Source = new Uri(file);
                t.RepeatBehavior = RepeatBehavior.Forever;
                video.Clock = t.CreateClock();

                // Start the video with audio muted.
                video.IsMuted = true;
                video.Clock.Controller.Begin();

                // Add the video to the list of possible puzzles.
                AddElementToPuzzleList(video);
            }
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Add an image or a movie into the puzzle list
        /// </summary>
        /// <param name="img"></param>
        private void AddElementToPuzzleList(UIElement img)
        {
            // Wrap the item in a Viewbox to constrain its size.
            Viewbox b = new Viewbox {Width = 200, Child = img};

            // Insert items at random, so the videos won't all be grouped 
            // at the bottom.
            puzzles.Items.Insert(random.Next(0, puzzles.Items.Count), b);
        }

        #endregion

        #region StartPuzzle and Change Difficulty

        //---------------------------------------------------------//
        /// <summary>
        /// Called when the selection changes on the ListBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void OnPuzzleSelected(object sender, SelectionChangedEventArgs args)
        {
            if (puzzles.SelectedItem == null) { return; }
        
            // The content of the SelectedItem is a Viewbox that 
            // contains the Visual we want to use for the puzzle.
            Visual newPuzzle = ((Viewbox)puzzles.SelectedItem).Child;
                       
            if (!selectionEnabled || newPuzzle == currentPuzzle)
            {
                return;
            }

            // Disable further selections until remove animation completes.
            selectionEnabled = false;

            // Start the puzzle that was just selected
            currentPuzzle = newPuzzle;
            BeginSelectedPuzzle(currentPuzzle, Direction.Left);
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Loads a puzzle and adds all the pieces into the ScatterView.
        /// </summary>
        private void BeginSelectedPuzzle(Visual puzzle, Direction fromDirection)
        {
            if (puzzle == null)
            {
                selectionEnabled = true;
                return;
            }

            // Get to a clean state
            Reset();
                
            // Load the selected puzzle into the ScatterView.
            LoadVisualAsPuzzle(puzzle, fromDirection);
                
            // If the selected puzzle is a video, unmute it.
            MediaElement media = puzzle as MediaElement;
            if (media != null)
            {
                media.IsMuted = false;
            }
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Creates puzzle pieces from a visual, and adds them into the ScatterView.
        /// </summary>
        /// <param name="visual"></param>
        void LoadVisualAsPuzzle(Visual visual, Direction fromDirection)
        {
            // The more columns/rows, the less each piece needs to overlap
            float rowOverlap = PuzzleManager.Overlap / rowCount;
            float colOverlap = PuzzleManager.Overlap / colCount;

            puzzleBrush = new VisualBrush(visual);

            // Tell the puzzle manager to load a puzzle with the specified dimensions
            puzzleManager.LoadPuzzle(colCount, rowCount);

            for (int row = 0; row < rowCount; row++)
            {
                for (int column = 0; column < colCount; column++)
                {
                    // Calculate the size of the rectangle that will be used to create a viewbox into the puzzle image.
                    // The size is specified as a percentage of the total image size.
                    float boxLeft = (float) column / (float)colCount;
                    float boxTop = (float) row / (float)rowCount;
                    float boxWidth = 1f / colCount;
                    float boxHeight = 1f / rowCount;

                    // Items in column 0 don't have any male puzzle parts on their side, all others do
                    if (column != 0)
                    {
                        boxLeft -= colOverlap;
                        boxWidth += colOverlap;
                    }

                    // Items in row 0 don't have any male puzzle parts on their top, all others do
                    if (row != 0)
                    {
                        boxTop -= rowOverlap;
                        boxHeight += rowOverlap;
                    }

                    // Make a visual brush based on the rectangle that was just calculated.
                    VisualBrush itemBrush = new VisualBrush(visual);
                    itemBrush.Viewbox = new Rect(boxLeft, boxTop, boxWidth, boxHeight);
                    itemBrush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;

                    // Get the shape of the piece
                    Geometry shape = GetPieceGeometry(column, row);

                    // Put the brush into a puzzle piece
                    PuzzlePiece piece = new PuzzlePiece( column + (colCount * row), shape, itemBrush);

                    // Add the PuzzlePiece to a ScatterViewItem
                    SSC.ScatterViewItem item = new SSC.ScatterViewItem();
                    item.Content = piece;

                    // Set the initial size of the item and prevent it from being resized
                    item.Width = Math.Round(piece.ClipShape.Bounds.Width, 0);
                    item.Height = Math.Round(piece.ClipShape.Bounds.Height, 0);
                    item.CanScale = false;

                    // Set the item's data context so it can use the piece's shape
                    Binding binding = new Binding();
                    binding.Source = piece;
                    item.SetBinding(ScatterViewItem.DataContextProperty, binding);

                    // Animate the item into view
                    AddPiece(item, fromDirection);
                }
            }
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Returns a geometry that represents the shape of the piece, determined by the piece's row and column.
        /// </summary>
        /// <param name="row">The piece's row</param>
        /// <param name="column">The piece's column</param>
        private Geometry GetPieceGeometry(int column, int row)
        {
            System.Windows.Shapes.Path path;
            if (row == 0)
            {
                if (column == 0) 
                {
                    path = (System.Windows.Shapes.Path)Resources["TopLeftCorner"]; 
                }
                else if (column == colCount - 1) 
                {
                    path = (System.Windows.Shapes.Path)Resources["TopRightCorner"];  
                }
                else 
                {
                    path = (System.Windows.Shapes.Path)Resources["TopEdge"];  
                }
            }
            else if (row == rowCount - 1)
            {
                if (column == 0)
                {
                    path = (System.Windows.Shapes.Path)Resources["BottomLeftCorner"]; 
                }
                else if (column == colCount - 1)
                {
                    path = (System.Windows.Shapes.Path)Resources["BottomRightCorner"]; 
                }
                else
                {
                    path = (System.Windows.Shapes.Path)Resources["BottomEdge"]; 
                }
            }
            else if (column == 0)
            {
                path = (System.Windows.Shapes.Path)Resources["LeftEdge"]; 
            }
            else if (column == colCount - 1)
            {
                path = (System.Windows.Shapes.Path)Resources["RightEdge"]; 
            }
            else
            {
                path = (System.Windows.Shapes.Path)Resources["Center"]; 
            }

            return path.Data.GetFlattenedPathGeometry();
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Called when the value of the slider is changed. Adjusts the rows/columns 
        /// of the puzzle according to the value of the slider and resets the puzzle 
        /// to have the new number of pieces.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void OnDifficultyChanged(object sender, RoutedEventArgs args)
        {
            if (Difficulty.Value == 1.0)
            {
                rowCount = 2;
                colCount = 2;
            }
            else if (Difficulty.Value == 2.0)
            {
                rowCount = 2;
                colCount = 3;
            }
            else if (Difficulty.Value == 3.0)
            {
                rowCount = 3;
                colCount = 3;
            }
            else
            {
                return;
            }

            // If a puzzle was selected, the new pieces should come in from the right
            if (currentPuzzle != null)
            {
                BeginSelectedPuzzle(currentPuzzle, Direction.Right);               
            }
        }

        //---------------------------------------------------------//
        /// <summary>
        /// returns the application to its initial state where all movies are 
        /// playing and muted, and no pieces are on the board. 
        /// </summary>
        void Reset()
        {
            // If a puzzle is currently loaded and it is a video, mute it
            if (puzzleBrush != null)
            {
                MediaElement media = puzzleBrush.Visual as MediaElement;
                if (media != null)
                {
                    media.IsMuted = true;
                }
            }

            // Remove all puzzle pieces
            RemoveAllPieces();
        }

        #endregion

        #region Puzzle Assembly

        //---------------------------------------------------------//
        /// <summary>
        /// Occurs when a manipulationCompleted event is fired.
        /// </summary>
        /// <remarks>
        /// An item has been moved. See if it is placed near another item that to which it can be joined.
        /// </remarks>
        /// <param name="sender">The object that raized the event.</param>
        /// <param name="args">The arguments for the event.</param>
        void OnManipulationCompleted(object sender, SSC.ContainerManipulationCompletedEventArgs args)
        {
            // Validate input
            ScatterViewItem item = args.OriginalSource as ScatterViewItem;
            if (item == null)
            {
                return;
            }

            // If there are any pieces the can be joined, then join them
            StartJoinIfPossible(item);
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Search the puzzle area for a ScatterViewItem that can be joined to the piece that was 
        /// passed as an argument. If a match is found, join the pieces.
        /// </summary>
        /// <param name="item">The piece that could potentially be joined to another piece.</param>
        private void StartJoinIfPossible(ScatterViewItem item)
        {
            // Compare this piece against all the other pieces in the puzzle
            foreach (ScatterViewItem potentialItem in scatter.Items)
            {
                // Don't even bother trying to join a piece to itself
                if (potentialItem == item)
                {
                    continue;
                }

                // See if the pieces are eligible to join
                if (puzzleManager.CanItemsJoin(item, potentialItem))
                {
                    // The pieces are eligible, join them
                    JoinItems(potentialItem, item);

                    // Only join one set of pieces per manipulation
                    break;
                }
            }
        }

        #endregion

        #region Animations

        //---------------------------------------------------------//
        /// <summary>
        /// Animates all current pieces off the side of the screen.
        /// </summary>
        private void RemoveAllPieces () 
        {
            if (scatter.Items.Count == 0)
            {
                selectionEnabled = true;
                return;
            }

            // Use a for loop here instead of a foreach so the variable used in the animation complete 
            // callback is not modified between the time the callback is hooked up and the time it is called.
            for (int i = 0; i < scatter.Items.Count; i++)
            {
                ScatterViewItem item = (ScatterViewItem)scatter.Items[i];
                PointAnimation remove = ((PointAnimation)Resources["RemovePiece"]).Clone();

                // Can't animate if center isn't set yet, which would happen if a piece has not yet been manipulated
                if (double.IsNaN(item.Center.X))
                {
                    item.Center = item.ActualCenter;
                }

                // Set up a callback that passes the ScatterViewItem that will be needed when the animation completes
                remove.Completed += delegate(object sender, EventArgs e)
                {
                    OnRemoveAnimationCompleted(item);
                };

                // Start the animation
                item.BeginAnimation(ScatterViewItem.CenterProperty, remove, HandoffBehavior.SnapshotAndReplace);
            }
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Called when a remove animation has been completed.
        /// </summary>
        /// <param name="item">The item that was animated</param>
        private void OnRemoveAnimationCompleted(ScatterViewItem item)
        {
            scatter.Items.Remove(item);
            selectionEnabled = true;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Adds a ScatterViewItem to the ScatterView, and animates it 
        /// on from the specified side of the screen (Left or Right).
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="fromDirection">The direction from which puzzle pieces enter.</param>
        private void AddPiece(ScatterViewItem item, Direction fromDirection)
        {
            // Add the piece to the ScatterView at the correct location
            Debug.Assert(fromDirection == Direction.Right || fromDirection == Direction.Left);
                        
            double screenHeight = RootLayout.ActualHeight;
            double screenWidth = RootLayout.ActualWidth;
            item.Center = fromDirection == Direction.Right ? new Point(screenWidth, screenHeight / 2) : new Point(-100, screenHeight / 2);
            item.Orientation = random.Next(0, 360);
            scatter.Items.Add(item);

            // Load the animation
            Storyboard add = ((Storyboard)Resources["AddPiece"]).Clone();
            
            foreach (AnimationTimeline animation in add.Children)
            {
                // If this is a double animation, it animates the item's orientation
                DoubleAnimation orientation = animation as DoubleAnimation;
                if (orientation != null)
                {
                    // Spin the orientation a little.
                    orientation.To = item.Orientation + random.Next(-135, 135);
                }

                // If this is a point animation, then it animates the item's center
                PointAnimation center = animation as PointAnimation;
                if (center != null)
                {
                    // Get a random point to animate the item to
                    center.To = new Point(random.Next(0, (int)(screenHeight + 5)), random.Next(0, (int)screenHeight));
                }
            }

            // Set up a callback that passes the ScatterViewItem that will be needed when the animation completes
            add.Completed += delegate(object sender, EventArgs e)
            {
                OnAddAnimationCompleted(item);
            };

            // Start the animation
            item.BeginStoryboard(add, HandoffBehavior.SnapshotAndReplace);
        }
        
        //---------------------------------------------------------//
        /// <summary>
        /// Called when an add animation is completed.
        /// </summary>
        /// <param name="item">The item that was just animated.</param>
        private static void OnAddAnimationCompleted(ScatterViewItem item)
        {
            // When the animation completes, the animation will no longer be the determining factor for the layout
            // of the item, it will revert to the value with the nextmost precedence. In this case, it will be the 
            // values assigned to the item's center and orientation. Set those values to their current values (while
            // the item is still under animation) so that when the animation completes, the item won't appear to jump.
            item.Center = item.ActualCenter;
            item.Orientation = item.ActualOrientation;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Animates two ScatterViewItems together and then merges the content of both items into one ScatterViewItem.
        /// </summary>
        /// <param name="pieceRemaining">The piece that will remain after the join.</param>
        /// <param name="pieceBeingRemoved">The piece that will be removed as a result of the join.</param>
        private void JoinItems(ScatterViewItem pieceRemaining, ScatterViewItem pieceBeingRemoved)
        {
            // Simultaneous joins on the same pieces (as in the case where two matching pieces are dropped next 
            // to each other at the same time) eventually remove both matching pieces. Make sure only one join
            // happens at one time.
            if (!joinInProgress)
            {
                joinInProgress = true;

                Storyboard join = ((Storyboard)Resources["JoinPiece"]).Clone();

                foreach (AnimationTimeline animation in join.Children)
                {
                    // If this is a double animation, then it animates the piece's orientation
                    DoubleAnimation orientation = animation as DoubleAnimation;
                    if (orientation != null)
                    {
                        orientation.To = pieceRemaining.ActualOrientation;
                        orientation.From = pieceBeingRemoved.ActualOrientation;

                        // If two pieces are close in orientation, but seperated by the 0/360 line (i.e. 3 and 357) then don't spin the piece all the way around
                        if (Math.Abs(pieceRemaining.ActualOrientation - pieceBeingRemoved.ActualOrientation) > 180)
                        {
                            orientation.To += orientation.From > orientation.To ? 360 : -360;
                        }
                    }

                    // If this is a point animation, then it animates the piece's center
                    PointAnimation center = animation as PointAnimation;
                    if (center != null)
                    {
                        center.To = puzzleManager.CalculateJoinAnimationDestination(pieceRemaining, pieceBeingRemoved);
                    }

                    // Can't animate values that are set to NaN
                    if (double.IsNaN(pieceBeingRemoved.Orientation))
                    {
                        pieceBeingRemoved.Orientation = pieceBeingRemoved.ActualOrientation;
                    }

                    // Set up a callback that passes the ScatterViewItems that will be needed when the animation completes
                    join.Completed += delegate(object sender, EventArgs e)
                    {
                        OnJoinAnimationCompleted(pieceBeingRemoved, pieceRemaining);
                    };

                    pieceBeingRemoved.BeginStoryboard(join);
                }
            }
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Called when an join animation is completed.
        /// </summary>
        /// <param name="pieceBeingRemoved">The item that should be removed after the join.</param>
        /// <param name="pieceRemaining">The item that will remain after the join.</param>
        private void OnJoinAnimationCompleted(ScatterViewItem pieceBeingRemoved, ScatterViewItem pieceRemaining)
        {
            if (scatter.Items.Contains(pieceBeingRemoved) && scatter.Items.Contains(pieceRemaining))
            {
                // Get the content for the joined piece
                PuzzlePiece joinedPiece = puzzleManager.JoinPieces((PuzzlePiece)pieceBeingRemoved.Content, (PuzzlePiece)pieceRemaining.Content);

                // When size changes, center also must be adjusted so the piece doesn't jump
                Vector centerAdjustment = puzzleManager.CalculateJoinCenterAdjustment((PuzzlePiece)pieceRemaining.Content, joinedPiece);
                centerAdjustment = PuzzleManager.Rotate(centerAdjustment, pieceRemaining.ActualOrientation);

                // Replace the item's content with the new group
                pieceRemaining.Content = joinedPiece;

                // Resize the item to the size of the piece
                pieceRemaining.Width = Math.Round(joinedPiece.ClipShape.Bounds.Width, 0);
                pieceRemaining.Height = Math.Round(joinedPiece.ClipShape.Bounds.Height, 0);

                // Adjust the center
                pieceRemaining.Center = pieceRemaining.ActualCenter + centerAdjustment;

                // Bind the item to the new piece
                Binding binding = new Binding();
                binding.Source = joinedPiece;
                pieceRemaining.SetBinding(ScatterViewItem.DataContextProperty, binding);
                pieceRemaining.SetRelativeZIndex(RelativeScatterViewZIndex.AboveInactiveItems);

                // Remove the old item from the ScatterView
                scatter.Items.Remove(pieceBeingRemoved);

                // Set this to false before StartJoinIfPossible. If there is another join, JoinPieces will set joinInProgress to true,
                // and then it will immediately be set back when StartJoinIfPossible returns.
                joinInProgress = false;

                // See if there are any other pieces eligible to join to the newly joined piece
                StartJoinIfPossible(pieceRemaining);
            }
            else
            {
                // Still want to set this to false, even if no join was performed
                joinInProgress = false;
            }
        }

        #endregion

        /// <summary>
        /// Examines a puzzle Visual to determine if it is a video
        /// and starts it with the specified muted state.
        /// </summary>
        /// <param name="visual">The puzzle Visual to examine.</param>
        /// <param name="muted">Determines with the video is muted or not.</param>
        private static void StartMediaItem(Visual visual, bool muted)
        {
            MediaElement video = visual as MediaElement;
            if (video != null)
            {
                video.IsMuted = muted;
                if (video.Clock.CurrentState != ClockState.Active)
                {
                    video.Clock.Controller.Begin();
                }
            }
        }

        /// <summary>
        /// Starts all the puzzle list items that are videos
        /// with the audio muted.
        /// </summary>
        private void StartAllMedia()
        {
            foreach (Viewbox box in puzzles.Items)
            {
                StartMediaItem(box.Child, true);
            }
        }

        /// <summary>
        /// Calls a delegate once for each movie in the puzzle.
        /// </summary>
        /// <param name="action">the delegate to call</param>
        private void ForEachMediaElement(Action<MediaElement> action)
        {
            foreach (Viewbox box in puzzles.Items)
            {
                MediaElement movie = box.Child as MediaElement;
                if (movie != null)
                {
                    action(movie);
                }
            }
        }
    }
}
