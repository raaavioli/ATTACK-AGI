using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace ScatterPuzzle
{
    class PuzzlePiece : System.Windows.Shapes.Shape
    {
        #region Private Members

        // The piece's shape
        private Geometry clipShape;
           
        // Contains the viewbox into the puzzle image
        private VisualBrush imageBrush;

        // Layout for the piece
        HashSet<int> pieces;

        #endregion

        #region Public Properties

        /// <summary>
        /// The shape of the puzzle piece.
        /// </summary>
        public Geometry ClipShape
        {
            get
            {
                return clipShape;
            }
        }

        /// <summary>
        /// The VisualBrush that contains the piece's viewbox.
        /// </summary>
        public VisualBrush ImageBrush
        {
            get
            {
                return imageBrush;
            }
        }

        /// <summary>
        /// The layout of the piece.
        /// </summary>
        public HashSet<int> Pieces
        {
            get
            {
                return pieces;
            }
        }

        #endregion

        #region Initalization

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pieceNumber">Which piece number this piece contains.</param>
        /// <param name="pieceShape">The shape of the pieces.</param>
        /// <param name="brush">The image and viewbox to use as this piece's visual.</param>
        public PuzzlePiece(int pieceNumber, Geometry pieceShape, VisualBrush brush) 
        {
            clipShape = pieceShape;
            imageBrush = brush;
            pieces = new HashSet<int>();
            pieces.Add(pieceNumber);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pieceShape">The shape of the pieces.</param>
        /// <param name="brush">The image and viewbox to use as this piece's visual.</param>
        /// <param name="pieceGrid">The grid that represents the layout of the piece numbers contained in this piece</param>
        public PuzzlePiece(Geometry pieceShape, VisualBrush brush, HashSet<int> pieceSet )
        {
            clipShape = pieceShape;
            imageBrush = brush;
            pieces = pieceSet;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Fill = imageBrush;
            Height = Math.Round(clipShape.Bounds.Height, 0);
            Width = Math.Round(clipShape.Bounds.Width, 0);
        }

        #endregion

        #region Shape Methods
       
        /// <summary>
        /// Implementation of abstract DefiningGeometry property.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                return ClipShape;
            }
        }

        #endregion
    }
}
