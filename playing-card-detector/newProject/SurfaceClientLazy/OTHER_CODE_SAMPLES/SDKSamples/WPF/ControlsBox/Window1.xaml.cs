using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Input;
using Microsoft.Surface.Presentation.Controls;

namespace ControlsBox
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : SurfaceWindow
    {
        private readonly ObservableCollection<ImageInfo> images = new ObservableCollection<ImageInfo>();
        private readonly ObservableCollection<ImageInfo> images2 = new ObservableCollection<ImageInfo>();

        public static readonly RoutedCommand ShowMessageCommand = new RoutedCommand("ShowMessage", typeof(Window1));

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Window1()
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            // insert images for the library containers
            SetupLibraryContainerImages();
          
            // listen for changes to the primary InteractiveSurfaceDevice.
            InteractiveSurface.PrimarySurfaceDevice.Changed += OnPrimarySurfaceDeviceChanged;

            // Update the content selector ListBox.
            UpdateContentSelector();

            // Initialize the routed command for element menu
            SetupCommandMessage();
        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Stop listening for InteractiveSurfaceDevice changes when the window closes.
            InteractiveSurface.PrimarySurfaceDevice.Changed -= OnPrimarySurfaceDeviceChanged;

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

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

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        /// <summary>
        /// Update the content selector ListBox in the handler.
        /// </summary>
        private void OnPrimarySurfaceDeviceChanged(object sender, DeviceChangedEventArgs e)
        {
            UpdateContentSelector();
        }

        /// <summary>
        /// Change the visibility of the TagVisualizer item based or whether Byte Tags are supported
        /// by the primary InteractiveSurfaceDevice or not.
        /// </summary>
        private void UpdateContentSelector()
        {
            TagVisualizerItem.Visibility =
                InteractiveSurface.PrimarySurfaceDevice.IsTagRecognitionSupported
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        /// <summary>
        /// Change the content of the display area to show the newly selected control.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event args.</param>
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SurfaceListBoxItem selectedItem = (SurfaceListBoxItem)ContentSelector.SelectedItem;
            Grid content = selectedItem.Tag as Grid;
            if (content != null)
            {
                foreach (SurfaceDragCursor cursor in SurfaceDragDrop.GetAllCursors(this))
                {
                    SurfaceDragDrop.CancelDragDrop(cursor);
                }

                ContentArea.Children.Clear();
                ContentArea.Children.Add(content);
            }
        }  
        
        /// <summary>
        /// Clear the strokes from the InkCanvas.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event args.</param>
        private void OnInkCanvasClearClick(object sender, RoutedEventArgs e)
        {
            SampleInkCanvas.Strokes.Clear();
        }
        
        /// <summary>
        /// Setup command message
        /// </summary>
        private void SetupCommandMessage()
        {
            CommandBinding showMessage = new CommandBinding(ShowMessageCommand, ShowMessage, null);
            this.CommandBindings.Add(showMessage);
        }
                
        /// <summary>
        /// Display the Element Menu Item that is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ShowMessage(object sender, ExecutedRoutedEventArgs args)
        {
            textMessage.Text = args.Parameter + " Clicked";
        }

        /// <summary>
        /// Add image and group name items to the image collection lists.
        /// </summary>
        private void SetupLibraryContainerImages()
        {
            string[] groups = { "Blue", "Green", "Orange", "Rhodamine" };

            //create two lists of images
            for (int i = 0; i <= 3; ++i)
            {
                for (int groupName = 0; groupName < 4; ++groupName)
                {
                    images.Add(new ImageInfo(groups[groupName] + i.ToString("0") + ".jpg", groups[groupName]));
                }
            }
            for (int i = 4; i <= 9; ++i)
            {
                for (int groupName = 0; groupName < 4; ++groupName)
                {
                    images2.Add(new ImageInfo(groups[groupName] + i.ToString("0") + ".jpg", groups[groupName]));
                }
            }

            //map the images to the first library container
            libraryContainer1.DataContext = "libraryContainer1";
            ICollectionView view = CollectionViewSource.GetDefaultView(images);
            view.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
            libraryContainer1.ItemsSource = view;

            //map the images to the second library container
            libraryContainer2.DataContext = "libraryContainer2";
            ICollectionView view2 = CollectionViewSource.GetDefaultView(images2);
            view2.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
            libraryContainer2.ItemsSource = view2;
        }

    }
}