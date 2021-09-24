using System;
using CoreInteractionFramework;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cloth.UI
{
    /// <summary>
    /// This class represents the view of the list box control in the Cloth sample.
    /// It uses the ListBoxStateMachine class to update its behavior.
    /// </summary>
    internal class ListBox : UIElement
    {
        // The UIElementStateMachine encapsulated by this view.
        private readonly ListBoxStateMachine listBoxStateMachine;

        // Default and pressed item images.
        private Texture2D itemDefault;
        private Texture2D itemHit;

        private const int itemWidth = 40;
        private const int itemHeight = 40;
        private const int iconWidth = 32;
        private const int iconHeight = 32;

        private const int iconMargin = 4;
        private const int glowSize = 5;

        private const float pixelsOfElasticity = 25;

        private readonly Vector2 iconScale;

        // ListBox background textures.
        private Texture2D minBackground;
        private Texture2D maxBackground;

        // The Textiles UIElement associated with this ListBox.
        // Needed to display preview textures.
        private readonly Textiles textiles;

        // The ScrollBar UIElement associated with this ListBox.
        public ScrollBar ScrollBar { get; private set; }

        private const float scrollBarBackgroundHeight = 34;

        // Button to minimize or Maximize list box.
        private readonly Button minMaxButton;
        private const int buttonWidth = 48;
        private const int buttonHeight = 25;

        // Textures for minMaxButton.
        private Texture2D maximizeDefault;
        private Texture2D maximizeHit;
        private Texture2D minimizeDefault;
        private Texture2D minimizeHit;


        // ListBox expand-contract animation paramerters.
        private ViewStates listBoxViewState = ViewStates.Minimized;

        private readonly TimeSpan animationDuration = TimeSpan.FromMilliseconds(300);
        private float animationSteps;
        private float targetWidth;
        private float widthStep;

        private float viewPortStartPosition;
        private float viewportStep;

        // Adjust LayerDepth by this amount to raise or lower sprites.
        private const float layerOffset = 0.002f;

        #region Constructors

        /// <summary>
        /// Create a ListBox UIElement contained within another UIElement
        /// </summary>
        /// <param name="parent">UIElement that contains the Listbox (required).</param>
        /// <param name="position">Position relative to parent's center (-0.5f .. 0.5f) </param>
        /// <param name="width">Width of the ListBox in pixels.</param>
        /// <param name="height">Height of the ListBox in pixels.</param>
        /// <param name="textiles">Textiles UIElement controlled by this list box</param>
        public ListBox(UIElement parent, Vector2 position, int width, int height, Textiles textiles)
            : this(parent.Game, parent.Controller, position.X, position.Y, width, height, parent, textiles)
        {
            // Empty.
        }

        /// <summary>
        /// Creates a Listbox UIElement that may or may not have a parent UIElement.
        /// </summary>
        /// <param name="game">XNA Game that contains this ListBox</param>
        /// <param name="contoller">UIController to associate with the ListBoxStateMachine.</param>
        /// <param name="x">X-coordinate of elements position (relative or screen).</param>
        /// <param name="y">Y-coordinate of elements position (relative or screen).</param>
        /// <param name="width">Width of the ListBox in pixels.</param>
        /// <param name="height">Height of the ListBox in pixels.</param>
        /// <param name="parent">UIElement that contains the Listbox.</param>
        /// <param name="textiles">Textiles UIElement controlled by this list box</param>
        public ListBox(Game game, UIController contoller, float x, float y, int width, int height,
                       UIElement parent, Textiles textiles)
            : base(game, contoller, new Vector2(x, y), width, height, parent)
        {
            listBoxStateMachine = new ListBoxStateMachine(Controller, (int)width, (int)height)
                                      {
                                          SelectionMode = SelectionMode.Single,
                                          Orientation = Orientation.Horizontal,
                                          HorizontalElasticity = 0.0f,
                                          VerticalElasticity = 0.0f,
                                          HorizontalViewportSize = 1f,
                                          VerticalViewportSize = 1f,
                                      };

            StateMachine = listBoxStateMachine;
            StateMachine.Tag = this;

            this.textiles = textiles;

            listBoxStateMachine.ItemStateChanged += OnItemStateChanged;

            iconScale = new Vector2((float)iconWidth / itemWidth, (float)iconHeight / itemHeight);

            UIContainer container = parent as UIContainer;
            if (container != null)
            {
                // Create the scrollbar and position it below the listbox.
                int offset = Convert.ToInt32(Top - parent.Top + Height);
                Vector2 position = UIElement.CenterHorizontal(offset, scrollBarBackgroundHeight, container);

                ScrollBar = new ScrollBar(parent, position, width, listBoxStateMachine,
                                          listBoxStateMachine.HorizontalScrollBarStateMachine);

                // Create the Maximze/Minimize button and position it above the listbox.
                offset = Convert.ToInt32(Top - parent.Top - buttonHeight);
                position = UIElement.CenterHorizontal(offset, (float) buttonHeight, Parent);
                minMaxButton = new Button(parent, position, buttonWidth, buttonHeight);
                minMaxButton.Name = "button";
                minMaxButton.AutoScaleTexture = false;
            }
            else
            {
                throw new InvalidOperationException(Properties.Resources.ListBoxShouldBeInUIContainer);
            }
        }

        #endregion Constructors

        /// <summary>
        /// Read-only property that returns the item width used by this ListBox class.
        /// </summary>
        public static int ItemWidth
        {
            get { return itemWidth; }
        }

        /// <summary>
        /// Read-only property that returns the item height used by this ListBox class.
        /// </summary>
        public static int ItemHeight
        {
            get { return itemHeight; }
        }

        /// <summary>
        /// Read-only property that returns the minimum width used by this ListBox class.
        /// </summary>
        public static int MinimumListBoxWidth
        {
            get { return ItemWidth * 3; }
        }


        /// <summary>
        /// Load all of the content for the list box.
        /// </summary>
        protected override void LoadContent()
        {
            itemDefault = TextureFromFile(@"Content\ItemDefault.png");
            itemHit = TextureFromFile(@"Content\ItemHit.png");
            minBackground = TextureFromFile(@"Content\MinBackground.png");
            maxBackground = TextureFromFile(@"Content\MaxBackground.png");

            textiles.Initialize();

            animationSteps = (float)(animationDuration.TotalMilliseconds/Game.TargetElapsedTime.TotalMilliseconds);
            widthStep = (float) (maxBackground.Width - minBackground.Width)/animationSteps;

            listBoxStateMachine.Items.Clear();

            // Populate the listbox.
            float horizontalSize = (float)itemWidth/listBoxStateMachine.NumberOfPixelsInHorizontalAxis;
            float verticalSize = (float)itemHeight/listBoxStateMachine.NumberOfPixelsInVerticalAxis;
            foreach (Textiles.Theme theme in Enum.GetValues(typeof(Textiles.Theme)))
            {
                Texture2D icon = textiles.GetIcon(theme);
                ListBoxStateMachineItem item = new ListBoxItem(icon, theme, horizontalSize, verticalSize);
                listBoxStateMachine.Items.Add(item);
            }


            // Initialize button images
            maximizeDefault = TextureFromFile(@"Content\MaximizeDefault.png");
            maximizeHit = TextureFromFile(@"Content\MaximizeHit.png");
            minimizeDefault = TextureFromFile(@"Content\MinimizeDefault.png");
            minimizeHit = TextureFromFile(@"Content\MinimizeHit.png");

            minMaxButton.DefaultImage = maximizeDefault;
            minMaxButton.PressedImage = maximizeHit;

            base.LoadContent();
        }

        /// <summary>
        /// Reduces the size of the ListBox viewport.
        /// </summary>
        public void Minimize()
        {
            targetWidth = MinimumListBoxWidth;
            listBoxViewState = ViewStates.Contracting;
            ScrollBar.Reveal(animationSteps);
        }

        /// <summary>
        /// Increases the size of the ListBox viewport.
        /// </summary>
        public void Maximize()
        {
            targetWidth = itemWidth * listBoxStateMachine.Items.Count;
            // Because of Elasticity the value out can be outside the range [0,1]
            // But we can only restore a value between [0, 1 - HorizontalViewPortsize].
            viewPortStartPosition = MathHelper.Clamp(listBoxStateMachine.HorizontalViewportStartPosition,
                                                     0f,
                                                     1f - listBoxStateMachine.HorizontalViewportSize);
            viewportStep = viewPortStartPosition/animationSteps;
            listBoxViewState = ViewStates.Expanding;
            ScrollBar.Hide(animationSteps);
        }

        /// <summary>
        /// Handles updating the flag texture on the selected cloth when the ListBox item selected changes.
        /// </summary>
        private void OnItemStateChanged(object sender, ListBoxStateMachineItemEventArgs e)
        {
            ListBoxItem item = e.Item as ListBoxItem;
            if (item != null && item.IsSelected)
            {
                textiles.CurrentTheme = item.Theme;
            }
        }

        /// <summary>
        /// Updates the ListBox state.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (minMaxButton.GotClicked)
            {
                // Prohibit state change if scrollbar is selected.
                if (ScrollBar.StateMachine.SelectedPart == ScrollBarPart.None)
                {
                    // Button will be unresponsive until animation completes.
                    if (listBoxViewState == ViewStates.Minimized)
                    {
                        Maximize();
                        minMaxButton.DefaultImage = minimizeDefault;
                        minMaxButton.PressedImage = minimizeHit;
                    }
                    if (listBoxViewState == ViewStates.Maximized)
                    {
                        Minimize();
                        minMaxButton.DefaultImage = maximizeDefault;
                        minMaxButton.PressedImage = maximizeHit;
                    }
                }
            }

            foreach (ListBoxStateMachineItem item in listBoxStateMachine.Items)
            {
                ListBoxItem listBoxItem = item as ListBoxItem;
                listBoxItem.Update();
            }

            if (listBoxViewState == ViewStates.Expanding)
            {
                float width = Width + widthStep;
                if (width >= targetWidth)
                {
                    // Expand animation complete.
                    width = targetWidth;
                    listBoxViewState = ViewStates.Maximized;
                    ScrollBar.Visible = false;
                    ScrollBar.Enabled = false;
                    ScrollBar.Active = false;
                }
                float viewportStart = listBoxStateMachine.HorizontalViewportStartPosition - viewportStep;
                viewportStart = MathHelper.Clamp(viewportStart, 0f, 1 - listBoxStateMachine.HorizontalViewportSize);
                listBoxStateMachine.HorizontalViewportStartPosition = viewportStart;
                Width = width;
                listBoxStateMachine.NumberOfPixelsInHorizontalAxis = Convert.ToInt32(Width);
            }

            if (listBoxViewState == ViewStates.Contracting)
            {
                float width = Width - widthStep;
                if (width <= targetWidth)
                {
                    // Contract animation complete.
                    width = targetWidth;
                    listBoxViewState = ViewStates.Minimized;
                    ScrollBar.Visible = true;
                    ScrollBar.Active = true;
                }
                Width = width;
                listBoxStateMachine.NumberOfPixelsInHorizontalAxis = Convert.ToInt32(Width);
                if (listBoxViewState == ViewStates.Minimized)
                {
                    listBoxStateMachine.HorizontalViewportStartPosition = viewPortStartPosition;
                }
                else
                {
                    float viewportStart = listBoxStateMachine.HorizontalViewportStartPosition + viewportStep;
                    viewportStart = MathHelper.Clamp(viewportStart, 0f, 1 - listBoxStateMachine.HorizontalViewportSize);
                    listBoxStateMachine.HorizontalViewportStartPosition = viewportStart;
                }
            }
            listBoxStateMachine.HorizontalElasticity =
                pixelsOfElasticity / listBoxStateMachine.NumberOfPixelsInHorizontalAxis;

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the ListBox.
        /// </summary>
        /// <remarks>Overrides Draw method from UIElement.</remarks>
        /// <param name="batch"></param>
        /// <param name="gameTime"></param>
        public override void Draw(SpriteBatch batch, GameTime gameTime)
        {
            Vector2 ScaleX = Vector2.One;
            if (listBoxViewState == ViewStates.Maximized)
            {
                batch.Draw(maxBackground, TransformedCenter, null, SpriteColor, ActualRotation,
                           CenterOf(maxBackground), ActualScale, SpriteEffects, LayerDepth);
            }
            else
            {
                ScaleX.X = (float)listBoxStateMachine.NumberOfPixelsInHorizontalAxis / minBackground.Width;
                batch.Draw(minBackground, TransformedCenter, null, SpriteColor, ActualRotation,
                           CenterOf(minBackground), ScaleX * ActualScale, SpriteEffects, LayerDepth);
            }
            foreach (ListBoxStateMachineItem item in listBoxStateMachine.Items)
            {
                ListBoxItem listBoxItem = item as ListBoxItem;
                if (listBoxItem != null && listBoxItem.IsVisible)
                {
                    if (listBoxItem.IsPressed || listBoxItem.IsAnimating)
                    {
                        DrawPressedItem(batch, listBoxItem);
                    }
                    else
                    {
                        DrawItem(batch, listBoxItem);
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the source and destination Rectangles to be used for drawing a list box item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="source">The source Rectangle.</param>
        /// <param name="destination">The destination Rectangle</param>
        /// <param name="clipLeft">Number of pixels to clip on left.</param>
        /// <param name="clipRight">Number of pixels to clip on right.</param>
        private void GetDrawingRectangles(ListBoxStateMachineItem item,
                                          out Rectangle source, out Rectangle destination,
                                          out int clipLeft, out int clipRight)
        {
            int x = Convert.ToInt32(Left + item.HorizontalStartPosition * item.Parent.NumberOfPixelsInHorizontalAxis);
            int y = Convert.ToInt32(Top);

            int width = itemDefault.Width;
            int height = itemDefault.Height;

            // Determine if the item should be clipped on the right.
            clipRight = 0;
            if (x + itemDefault.Width > Right)
            {
                clipRight = x + itemDefault.Width - Convert.ToInt32(Right);
                width -= clipRight;
            }

            // Determine if the item should be clipped on the left.
            int left = Convert.ToInt32(Left);
            clipLeft = 0;
            if (x < left)
            {
                clipLeft = left - x;
                width -= clipLeft;
                x = left;
            }

            source = new Rectangle(clipLeft, 0, width, height);
            destination = new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Draws the texture icon in the center of the list box item frame.
        /// </summary>
        /// <param name="batch">SpriteBatch used for Drawing.</param>
        /// <param name="icon">The texture icon to draw.</param>
        /// <param name="center">A Vector2 containing the center point where the Listbox frame will be drawn.</param>
        /// <param name="clipLeft">Number of pixels on left that the listbox frame will be trimmed.</param>
        /// <param name="clipRight">Number of pixels on right that the listbox frame will be trimmed.</param>
        private void DrawIcon(SpriteBatch batch, Texture2D icon, Vector2 center,
                              int clipLeft, int clipRight)
        {
            System.Diagnostics.Debug.Assert(clipLeft == 0 || clipRight == 0);

            Rectangle iconSource = new Rectangle(0, 0, iconWidth, iconHeight);

            // We don't need to handle clipping on both left and right sides
            // because the viewport will never be that small.
            if (clipRight > iconMargin)
            {
                clipRight -= iconMargin;
                iconSource = new Rectangle(0, 0, iconWidth - clipRight, iconHeight);
            }

            if (clipLeft > iconMargin)
            {
                clipLeft -= iconMargin;
                iconSource = new Rectangle(clipLeft, 0, iconWidth - clipLeft, iconHeight);
            }

            Vector2 origin = new Vector2(iconSource.Width / 2f, iconSource.Height / 2f);
            batch.Draw(icon, center, iconSource, SpriteColor, ActualRotation, origin,
                       iconScale, SpriteEffects, LayerDepth);

        }

        /// <summary>
        /// Draws a list box item.
        /// </summary>
        /// <param name="batch">SpriteBatch to use for drawing.</param>
        /// <param name="item">ListBoxStateMachineItem to draw.</param>
        private void DrawItem(SpriteBatch batch, ListBoxItem item)
        {
            Rectangle source;
            Rectangle destination;
            int clipLeft, clipRight;

            GetDrawingRectangles(item, out source, out destination, out clipLeft, out clipRight);

            // Draw the list box item icon.
            DrawIcon(batch, item.Icon, CenterOf(destination), clipLeft, clipRight);

            // Draw the list box item frame texture.
            batch.Draw(itemDefault, destination, source, SpriteColor, ActualRotation, Vector2.Zero,
                       SpriteEffects, LayerDepth);
        }

        /// <summary>
        /// Draws a list box item that is pressed.
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="item"></param>
        private void DrawPressedItem(SpriteBatch batch, ListBoxItem item)
        {
            Rectangle source;
            Rectangle destination;
            int clipLeft, clipRight;

            GetDrawingRectangles(item, out source, out destination, out clipLeft, out clipRight);

            Rectangle frameSource = new Rectangle(0, 0, itemHit.Width, itemHit.Height);
            Vector2 origin = CenterOf(frameSource);
            Vector2 center = CenterOf(destination);

            if (clipRight > 0)
            {
                frameSource = new Rectangle(0, 0,
                                            itemHit.Width - clipLeft - clipRight - glowSize,
                                            itemHit.Height);
                center.X += Convert.ToInt32(clipRight/2f);
            }

            if (clipLeft > 0)
            {
                frameSource = new Rectangle(clipLeft + glowSize, 0,
                                            itemHit.Width - clipLeft - clipRight - glowSize,
                                            itemHit.Height);
                center.X += Convert.ToInt32(clipLeft/2f) + glowSize;
            }

            // Draw the list box item icon.
            DrawIcon(batch, item.Icon, CenterOf(destination), clipLeft, clipRight);

            // Draw the list box item frame texture.
            float zOrder = MathHelper.Clamp(LayerDepth - layerOffset, 0f, 1f);
            batch.Draw(itemHit, center, frameSource, SpriteColor, ActualRotation, origin,
                       item.AnimationScale * ActualScale, SpriteEffects, zOrder);
        }

        /// <summary>
        /// Calculates the HitTestDetails for the ListBoxStatemachine associated with this ListBox.
        /// </summary>
        /// <param name="touch"></param>
        /// <param name="captured">Boolean indicating that the touch was previously captured.</param>
        /// <returns>ListBoxHitTestDetails representing the location of the touch.</returns>
        public override IHitTestDetails HitTestDetails(TouchPoint touch, bool captured)
        {
            Vector2 touchVector = Vector2.Transform(new Vector2(touch.CenterX, touch.CenterY), ScreenTransform);

            float x = MathHelper.Clamp((touchVector.X - Left) / Width, 0f, 1f);
            float y = MathHelper.Clamp((touchVector.Y - Top) / Height, 0f, 1f);

            return new ListBoxHitTestDetails(x, y);
        }

    }
}
