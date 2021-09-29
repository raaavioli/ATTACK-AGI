using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xna.Framework.Audio;
using SurfaceControls = Microsoft.Surface.Presentation.Controls;


namespace GrandPiano
{
    /// <summary>
    /// User control representing the piano keyboard
    /// </summary>
    public partial class PianoKeyboard : UserControl, IDisposable
    {
        // Audio API components.
        private AudioEngine audioEngine;
        private WaveBank waveBank;
        private SoundBank soundBank;

        private bool disposed;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PianoKeyboard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Adds event handlers that change piano key images and play audio.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            AddHandlers();

            LoadAudioContent ( );
        }
        /// <summary>
        /// Adds event handlers.
        /// </summary>
        private void AddHandlers()
        {
            AddHandler("C7");
            AddHandler("C_7");
            AddHandler("D7");
            AddHandler("D_7");
            AddHandler("E7");
            AddHandler("F7");
            AddHandler("F_7");
            AddHandler("G7");
            AddHandler("G_7");
            AddHandler("A7");
            AddHandler("A_7");
            AddHandler("B7");
            AddHandler("C6");
            AddHandler("C_6");
            AddHandler("D6");
            AddHandler("D_6");
            AddHandler("E6");
            AddHandler("F6");
            AddHandler("F_6");
            AddHandler("G6");
            AddHandler("G_6");
            AddHandler("A6");
            AddHandler("A_6");
            AddHandler("B6");
        }

        /// <summary>
        /// Loads the XACT files that contain the audio content
        /// </summary>
        private void LoadAudioContent()
        {
            string filename = System.Windows.Forms.Application.ExecutablePath;
            string path = System.IO.Path.GetDirectoryName(filename) + "\\Audio\\";

            try
            {
                audioEngine = new AudioEngine(path + "PianoSounds.xgs");
                waveBank = new WaveBank(audioEngine, path + "PianoSounds.xwb");
                soundBank = new SoundBank(audioEngine, path + "PianoSounds.xsb");
            }
            
            catch(ArgumentException)
            {
                // Fail silently
                waveBank = null;
                soundBank = null;
                audioEngine = null;
            }

            catch ( InvalidOperationException )
            {
                // Fail silently
                waveBank = null;
                soundBank = null;
                audioEngine = null;
            }

            catch ( System.IO.FileNotFoundException )
            {
                // Fail silently
                waveBank = null;
                soundBank = null;
                audioEngine = null;
            }

            catch ( Microsoft.Xna.Framework.Audio.NoAudioHardwareException )
            {
                // Fail silently
                waveBank = null;
                soundBank = null;
                audioEngine = null;
            }
        }

        /// <summary>
        /// Adds a handler for the given key.
        /// </summary>
        /// <param name="key">a piano key to add handlers</param>
        private void AddHandler(string key)
        {
            FrameworkElement pianoKey = GetElement(key);

            // add OnIsPressed changed
            DependencyPropertyDescriptor.FromProperty(SurfaceControls.SurfaceButton.IsPressedProperty,
                typeof(SurfaceControls.SurfaceButton)).AddValueChanged(pianoKey, OnIsPressedChanged);

            // add MouseLeftButton Down and Up
            pianoKey.MouseLeftButtonDown += OnMouseLeftButtonDown;
            pianoKey.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        /// <summary>
        /// Event handler for the MouseDown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StateChanged((SurfaceControls.SurfaceButton)sender, true);
        }

        /// <summary>
        /// Event handler for the MouseUp.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StateChanged((SurfaceControls.SurfaceButton)sender, false);
        }

        /// <summary>
        /// Event handler for the IsPressedChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIsPressedChanged(object sender, EventArgs e)
        {
            SurfaceControls.SurfaceButton pianoKey = (SurfaceControls.SurfaceButton)sender;
            bool isPressed = pianoKey.IsPressed;

            if (isPressed)
            {
                // play the note only if a touch device is over or
                // the mouse is over with the left key pressed,
                // pressing the left key is handled separately by the OnMouseLeftButtonDown and Up
                // event handlers
                if (pianoKey.AreAnyTouchesOver || pianoKey.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    StateChanged(pianoKey, true);
                }
            }
            else
            {
                // draw unpressed
                StateChanged(pianoKey, false);
            }
        }


        /// <summary>
        /// Handles piano key state change.
        /// </summary>
        /// <param name="pianoKey"></param>
        /// <param name="isPressed"></param>
        private void StateChanged(SurfaceControls.SurfaceButton pianoKey, bool isPressed)
        {
            if (isPressed)
            {
                // play the note and draw piano key in the pressed state
                PlayNote(pianoKey);
                DrawPianoKey(pianoKey, true);
            }
            else
            {
                // draw the piano key in unpressed state
                DrawPianoKey(pianoKey, false);
            }
        }


        /// <summary>
        /// Plays the tone.
        /// </summary>
        /// <param name="pianoKey"></param>
        private void PlayNote(FrameworkElement pianoKey)
        {
            // XNA applications are expected to call AudioEngine.Update() once per frame.
            // This isn't an XNA application, but the Update method should still be called
            // to make sure the audio engine stays internally consistent.
            if (audioEngine != null)
            {
                audioEngine.Update();

                if (soundBank != null)
                {
                    Cue cue = soundBank.GetCue(pianoKey.Name);
                    cue.Play();
                }
            }
        }

        /// <summary>
        /// Returns an element for the given name.
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        private FrameworkElement GetElement(string elementName)
        {
            return (FrameworkElement)keys.FindName(elementName);
        }

        /// <summary>
        /// Draws the given pianoKey in pressed or unpressed state.
        /// </summary>
        /// <param name="pianoKey"></param>
        private void DrawPianoKey(FrameworkElement pianoKey, bool isPressed)
        {
            // draw the key in the pressed state
            string key = pianoKey.Name;

            Visibility pressedImageVisibility = isPressed ? Visibility.Visible : Visibility.Hidden;
            Visibility normalImageVisibility = isPressed ? Visibility.Hidden : Visibility.Visible;

            // sharp/flat tones
            if (key.Contains("_"))
            {
                GetElement("key" + key + "x").Visibility = pressedImageVisibility;
                GetElement("key" + key).Visibility = normalImageVisibility;
            }

            // normal tones
            else
            {
                GetElement("key" + key + "xtop").Visibility = pressedImageVisibility;
                GetElement("key" + key + "xbottom").Visibility = pressedImageVisibility;
                GetElement("key" + key + "top").Visibility = normalImageVisibility;
                GetElement("key" + key + "bottom").Visibility = normalImageVisibility;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Called when the object is being disposed
        /// </summary>
        /// <remarks>
        /// This object will be cleaned up by the Dispose method. Therefore,
        /// GC.SupressFinalize is called to take this object off the 
        /// finalization queue and prevent finalization code for this object
        /// from executing a second time.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">
        /// True if being called from user code, false if being called 
        /// from the runtime
        /// </param>
        private void Dispose(bool disposing)
        {
            if(!disposed)
            {
                // If disposing equals true, dispose all managed members
                if(audioEngine != null && disposing)
                {
                    waveBank.Dispose();
                    soundBank.Dispose(); 
                    audioEngine.Dispose();
                }
                disposed = true;
            }
        }

        #endregion
    }
}