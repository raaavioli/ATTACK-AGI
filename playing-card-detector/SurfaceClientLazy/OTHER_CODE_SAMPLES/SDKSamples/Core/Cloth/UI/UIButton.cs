using System;
using CoreInteractionFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cloth.UI
{
    /// <summary>
    /// This class provides a generalized view of a button control based on the UIElement class
    /// and the ButtonStateMachine class.
    /// </summary>
    public class UIButton : UIElement
    {
        // The ButtonStateMachine associated with this button.
        private readonly ButtonStateMachine buttonStateMachine;

        // Names of image files associated with this button.
        private string defaultImageFile;
        private string pressedImageFile;

        /// <summary>
        /// Get or set the texture used to display the button in the default state.
        /// </summary>
        public Texture2D DefaultImage { get; set; }

        /// <summary>
        /// Get or set the texture used to display the button in the pressed state.
        /// </summary>
        public Texture2D PressedImage { get; set; }

        /// <summary>
        /// Is triggered when a ButtonStateMachine Click occurs.  A Click is determined by
        /// the ClickMode property.  ClickModes include Press, Hover, and Release. Core
        /// events are triggered between the call to IController.Update and its return.  In
        /// other words, core events are triggered after the call to IController.Update, but
        /// before the IController.Update method completes.
        /// </summary>
        public event EventHandler Click;

        #region Constructors

        /// <summary>
        /// Creates a UIButton element.
        /// </summary>
        /// <param name="game">XNA Game that owns this element.</param>
        /// <param name="controller">UIController associated with the state machine for this element.</param>
        /// <param name="positionX">Element position X-coordinate (relative or screen coordinate).</param>
        /// <param name="positionY">Element position Y-coordinate (relative or screen coordinate).</param>
        /// <param name="width">Desired width of the element in pixels.</param>
        /// <param name="height">Desired height of the element in pixels.</param>
        /// <param name="parent">The containing UIElement for this button (optional).</param>
        /// <remarks>If parent is not null (x,y) position coordinates are expected to be relative to the parent,
        /// otherwise they are interpreted as screen coordinates.</remarks>
        public UIButton(Game game, UIController controller, float positionX, float positionY, int width, int height, UIElement parent)
            : base(game, controller, null, new Vector2(positionX, positionY), width, height, parent)
        {
            buttonStateMachine = new ButtonStateMachine(controller, (int)width, (int)height);
            buttonStateMachine.Click += OnButtonClick;

            base.StateMachine = buttonStateMachine;
            StateMachine.Tag = this;
        }

        /// <summary>
        /// Creates a UIButton element.
        /// </summary>
        /// <param name="parent">The containing UIElement for this button (required).</param>
        /// <param name="position">The position of this button's center relative to its parent center.</param>
        /// <param name="width">The render width of the button in pixels.</param>
        /// <param name="height">The render height of this button in pixels.</param>
        public UIButton(UIElement parent, Vector2 position, int width, int height)
            : base(parent, position, width, height)
        {
            buttonStateMachine = new ButtonStateMachine(Controller, (int)width, (int)height);
            buttonStateMachine.Click += OnButtonClick;
            base.StateMachine = buttonStateMachine;
            StateMachine.Tag = this;
        }

        #endregion Constructors

        /// <summary>
        /// Returns the ButtonStateMachine associated with this UIButton.
        /// </summary>
        /// <remarks>Hides the base property that returns a UIElementStateMachine.</remarks>
        public new ButtonStateMachine StateMachine
        {
            get { return buttonStateMachine; }
        }

        /// <summary>
        /// Sets the file names used for the default and pressed image textures.
        /// </summary>
        /// <remarks>
        /// The files will not be loaded until LoadContent() is called.
        /// After LoadContent() has been called the files will be loaded immediately.
        /// </remarks>
        /// <param name="defaultImage">Name of the graphics file to used as the default button image</param>
        /// <param name="pressedImage">Name of the graphics file to used as the pressed button image</param>
        public void SetImageNames(string defaultImage, string pressedImage)
        {
            defaultImageFile = defaultImage;
            pressedImageFile = pressedImage;
            if (initialized)
            {
                LoadImages();
            }
        }

        #region ButtonStateMachine Pass Through.

        /// <summary>
        /// Read-Writable property representing the value that determins what action causes a
        /// click.  The default action is Release.  In Release mode the click event is triggered
        /// when the touch is removed (button click up).
        /// </summary>
        /// <returns>The current enum value that identifies what causes a Button click.</returns>
        public ClickMode ClickMode
        {
            get
            {
                return buttonStateMachine.ClickMode;
            }
            set
            {
                buttonStateMachine.ClickMode = value;
            }
        }

        /// <summary>
        /// Read-only property can be used to verify that a Click event
        /// occured in the current Update.  This property can be used instead of using the
        /// OnClick event by checking it after UIController.Update is called.
        /// </summary>
        /// <returns>True if button was clicked within the current Update cycle.</returns>
        public bool GotClicked
        {
            get { return buttonStateMachine.GotClicked; }
        }

        /// <summary>
        /// Read-only property that can be used to verify if a Button is pressed.  A Button
        /// should be checked each time after UIController.Update is called if not using the
        /// Pressed event.
        /// </summary>
        /// <returns>True if there is a touch captured to the button.</returns>
        public bool IsPressed
        {
            get { return buttonStateMachine.IsPressed; }
        }

        #endregion

        /// <summary>
        /// Handles the Click event sent from the buttonStateMachine.
        /// Raises the Click event for clients of this button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonClick(object sender, EventArgs e)
        {
            EventHandler click = Click;
            if (click != null)
            {
                click(this, e);
            }
        }

        /// <summary>
        /// Load all of the content for the button.
        /// </summary>
        protected override void LoadContent()
        {
            LoadImages();
            base.LoadContent();
        }

        /// <summary>
        /// Loads the default and pressed image files.
        /// </summary>
        private void LoadImages()
        {
            if (defaultImageFile != null)
            {
                DefaultImage = TextureFromFile(defaultImageFile);
            }
            if (pressedImageFile != null)
            {
                PressedImage = TextureFromFile(pressedImageFile);
            }
            Texture = buttonStateMachine.IsPressed ? PressedImage : DefaultImage;
        }

        /// <summary>
        /// Updates the button element.
        /// </summary>
        /// <remarks>Extends the Update method from UIElement.</remarks>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Texture = buttonStateMachine.IsPressed ? PressedImage : DefaultImage;
            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the button element.
        /// </summary>
        /// <remarks>Overrides the Draw method from UIElement.</remarks>
        /// <param name="batch"></param>
        /// <param name="gameTime"></param>
        public override void Draw(SpriteBatch batch, GameTime gameTime)
        {
            Vector2 origin = CenterOf(Texture);
            batch.Draw(Texture, TransformedCenter, null, SpriteColor, ActualRotation, origin,
                       ActualScale, SpriteEffects, LayerDepth);
        }
    }

}
