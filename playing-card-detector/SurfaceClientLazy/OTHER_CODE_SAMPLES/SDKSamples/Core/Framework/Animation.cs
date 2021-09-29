using System;
using System.Diagnostics;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Creates an Animation of type float. 
    /// </summary>
    internal class Animation
    {
        private readonly float from;
        private float to;
        private bool isCompleted;
        private readonly TimeSpan duration;
        private readonly Stopwatch stopwatch;
        private bool isPlaying;

        /// <summary>
        /// Gets the starting position of this animation.
        /// </summary>
        public float From
        {
            get { return from; }
        }

        /// <summary>
        /// Gets the ending position of this animation.
        /// </summary>
        public float To
        {
            get { return to; }
            set { to = value; }
        }

        /// <summary>
        /// Gets true if it has completed, false otherwise.
        /// </summary>
        public bool IsCompleted
        {
            get { return isCompleted; }
        }

        /// <summary>
        /// Is the animation currently playing
        /// </summary>
        public bool IsPlaying
        {
            get { return isPlaying; }
        }

        /// <summary>
        /// The remaining time left in this animation.
        /// </summary>
        public TimeSpan RemainingTime
        {
            get
            {
                TimeSpan remaining = duration - stopwatch.Elapsed;

                return remaining.TotalMilliseconds < 0 ? new TimeSpan() : remaining;
            }
        }

        float? current = null;
        long currentTime;

        /// <summary>
        /// Gets the current position of the animation.
        /// </summary>
        public float Current
        {
            get
            {
                currentTime = stopwatch.ElapsedMilliseconds;

                if (stopwatch.Elapsed > duration)
                {
                    stopwatch.Stop();

                    // Set currentTime to max out at duration so it doesn't mess up calculations.
                    currentTime = (long)duration.TotalMilliseconds;

                    // Set this back to not having a value so that it is recalculated one last time.
                    current = null;
                    
                    isCompleted = true;
                    isPlaying = false;
                }

                // Get the time if we don't have it or if it's being updated.
                if (stopwatch.IsRunning || !current.HasValue)
                {
                    if (duration.TotalMilliseconds * currentTime != 0)
                    {
                        current = (float)((to - from) / duration.TotalMilliseconds * currentTime + from);
                    }
                }

                if (current.HasValue)
                {
                    return current.Value;
                }
                else
                {
                    return 0f;
                }
            }
        }

        /// <summary>
        /// Gets the duration of this animation.
        /// </summary>
        public TimeSpan Duration
        {
            get { return duration; }
        }

        /// <summary>
        /// Creates an animation of types int, double or float.  
        /// </summary>
        /// <param name="from">The starting position.</param>
        /// <param name="to">The ending position.</param>
        /// <param name="duration">The time it takes to get from start to end.</param>
        public Animation(float from, float to, TimeSpan duration)
        {
            stopwatch = new Stopwatch();
            this.current = this.from = from;
            this.to = to;
            this.duration = duration;
        }

        /// <summary>
        /// Starts the animation.  If paused starts from the point where it is paused.
        /// </summary>
        public void Play()
        {
            stopwatch.Start();
            isPlaying = true;
            isCompleted = false;
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        public void Pause()
        {
            if (isPlaying)
            {
                stopwatch.Stop();
                isPlaying = false;
                isCompleted = false;
            }
        }

        /// <summary>
        /// Stops the animation and resets it to the begining. 
        /// </summary>
        public void Stop()
        {
            if (isPlaying)
            {
                stopwatch.Stop();
                isPlaying = false;
                isCompleted = false;
            }

            stopwatch.Reset();
        }
    }
}
