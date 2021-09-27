using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.Surface.Presentation.Controls;

namespace ScatterPuzzle
{
    //---------------------------------------------------------//
    /// <summary>
    /// Represents the possible directions in which joins can happen.
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left, 
        Right,
        None
    }

    //---------------------------------------------------------//
    /// <summary>
    /// Maintains puzzle properties and handles calculations for pieces that
    /// exist in the puzzle that require information about where specific pieces
    /// exist/should exist in relation to one another.
    /// </summary>
    class PuzzleManager
    {
        #region Private Fields

        // Sizes
        private int columns;
        private int rows;
        private const float overlap = 0.3333333333f;
        private const int edgeLength = 150;
        private static readonly Vector halfEdge = new Vector(75, 75);

        // Angles
        private const double angleRight = 0; // Initalized to 0.0
        private const double angleUp = 90;
        private const double angleLeft = 180;
        private const double angleDown = 270;

        // Thresholds
        private const double orientationThreshold = 30;
        private const double lowerDistanceThreshold = 75;
        private const double upperDistanceThreshold = 225;

        #endregion

        #region Public Properties

        //---------------------------------------------------------//
        /// <summary>
        /// the percentage of the piece that overlaps another piece when those pieces are joined.
        /// </summary>
        public static float Overlap
        {
            get
            {
                return overlap;
            }
        }

        #endregion

        #region Initalization

        //---------------------------------------------------------//
        /// <summary>
        /// Load a puzzle.
        /// </summary>
        /// <param name="puzzleColumns">The number of columns in the puzzle.</param>
        /// <param name="puzzleRows">The number of rows in the puzzle.</param>
        public void LoadPuzzle(int puzzleColumns, int puzzleRows)
        {
            columns = puzzleColumns;
            rows = puzzleRows;
        }

        #endregion

        #region Piece Comparison

        //---------------------------------------------------------//
        /// <summary>
        /// Determines if two items can join based on their position, orientation, and content.
        /// </summary>
        /// <param name="itemJoining">The item that would be joined to the other item<./param>
        /// <param name="itemRemaining">The item to which the first item would be joined.</param>
        /// <returns>True if the pieces can be joined, false otherwise.</returns>
        public bool CanItemsJoin(ScatterViewItem itemJoining, ScatterViewItem itemRemaining)
        {
            // Comparing the item's orientations is the easiest check, so do that first
            if (!AreOrientationsWithinThreshold(itemJoining.ActualOrientation, itemRemaining.ActualOrientation))
            {
                return false;
            }

            PuzzlePiece pieceRemaining = (PuzzlePiece)itemRemaining.Content;
            PuzzlePiece pieceJoining = (PuzzlePiece)itemJoining.Content;

            // Get a hash set that contains all the piece numbers that could join to pieceRemaining
            HashSet<int> potentialMatches = new HashSet<int>();
            foreach (int i in pieceRemaining.Pieces)
            {
                if (i % columns > 0) potentialMatches.Add(i - 1);
                if ((i + 1) % columns > 0) potentialMatches.Add(i + 1);
                // Don't bother checking if the column is valid, it will get worked out in the intersect that happens next anyway
                potentialMatches.Add(i - columns);
                potentialMatches.Add(i + columns);
            }

            // Get all the piece numbers in pieceJoining that are also in potentialMatches
            potentialMatches.IntersectWith(pieceJoining.Pieces);

            // If there are no potential matches, then the pieces can't join
            if (potentialMatches.Count == 0)
            {
                return false;
            }

            // Check all piece numbers in potentialMatches against all piece numbers in 
            // PieceRemaining to see if any pairs are positioned within the join threshold
            foreach (int pieceNumJoining in potentialMatches)
            {
                // See if the potential match is an actual match, and if it is, what direction the match is in
                Direction correctDirection = Direction.None;
                int pieceNumRemaining = -1;
                if (pieceNumJoining % columns != 0 && pieceRemaining.Pieces.Contains(pieceNumJoining-1))
                {
                    correctDirection = Direction.Left;
                    pieceNumRemaining = pieceNumJoining-1;
                }
                else if ((pieceNumJoining + 1) % columns != 0 && pieceRemaining.Pieces.Contains(pieceNumJoining + 1))
                {
                    correctDirection = Direction.Right;
                    pieceNumRemaining = pieceNumJoining+1;
                }
                else if (pieceNumJoining / columns > 0 && pieceRemaining.Pieces.Contains(pieceNumJoining - columns))
                {
                    correctDirection = Direction.Up;
                    pieceNumRemaining = pieceNumJoining-columns;
                } 
                else if (pieceNumJoining / columns < rows - 1)
                {
                    correctDirection = Direction.Down;
                    pieceNumRemaining = pieceNumJoining+columns;
                }

                // If there isn't a good join direction, the pieces cna't join
                if (correctDirection == Direction.None)
                {
                    return false;
                }

                // The next set of calculations use Center instead of ActualCenter. There are 2 reasons for this:
                //  - Center is guaranteed to be set during the piece's animation onto the ScatterView
                //  - Center is potentially more accurate than ActualCenter, since this method can be called
                //    after Center is modified, but before a layout pass can update ActualCenter

                // Get the center of pieceNumJoining. This may or may not be the center of the ScatterViewItem that contains them
                Vector pieceCenterOffset = GetPieceNumberCenterInScreenUnits(pieceJoining, pieceNumJoining);
                Vector itemCenterOffset = GetAbsolutePieceCenterInScreenUnits (pieceJoining);
                Vector rawCenterOffset = pieceCenterOffset - itemCenterOffset;
                Vector rotatedCenteroffset = Rotate(rawCenterOffset, itemJoining.ActualOrientation);
                Vector pieceJoiningCenter = new Vector(itemJoining.Center.X, itemJoining.Center.Y) + rotatedCenteroffset;

                // Get the center of pieceNumRemaining. This may or may not be the center of the ScatterViewItem that contains them
                pieceCenterOffset = GetPieceNumberCenterInScreenUnits(pieceRemaining, pieceNumRemaining);
                itemCenterOffset = GetAbsolutePieceCenterInScreenUnits(pieceRemaining);
                rawCenterOffset = pieceCenterOffset - itemCenterOffset;
                rotatedCenteroffset = Rotate(rawCenterOffset, itemRemaining.ActualOrientation);
                Vector pieceRemainingCenter = new Vector(itemRemaining.Center.X, itemRemaining.Center.Y) + rotatedCenteroffset;

                // Get the vector of where pieceNumRemaining is relative to where pieceNumJoining is.
                Vector vectorBetweenItems = pieceRemainingCenter - pieceJoiningCenter;

                //// Determine the angle between the centers of the pieces 
                double joinAngle = Vector.AngleBetween(vectorBetweenItems, new Vector(1, 0));
                double angleBetween = joinAngle + itemJoining.ActualOrientation;

                // See if there is a direction for which the angle between the pieces is within the threshold for that direction
                Direction joinDirection = DetermineJoinDirection(ConstrainOrientation(angleBetween));

                // If the angles are close enough and the distance is within the threshold, then the pieces can be joined.
                if (joinDirection == correctDirection && IsDistanceWithinThreshold(vectorBetweenItems))
                {
                    return true;
                }
            }
            return false;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Gets a piece's top left relative to the puzzle.
        /// </summary>
        /// <param name="piece">The puzzle piece.</param>
        /// <returns>A vector that reprsents the top left of the piece, measured in pieces.</returns>
        private Vector GetPieceTopLeftInPieceUnits(PuzzlePiece piece)
        {
            Vector offset = new Vector(int.MaxValue, int.MaxValue);
            foreach (int i in piece.Pieces)
            {
                int row = GetPieceRow(i);
                int col = GetPieceColumn(i);

                if (col < offset.X)
                {
                    offset.X = col;
                }
                if (row < offset.Y )
                {
                    offset.Y = row;
                }
            }
            
            return offset;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Gets a piece's top left relative to the puzzle.
        /// </summary>
        /// <param name="piece">The puzzle piece.</param>
        /// <returns>A vector that reprsents the top left of the piece, measured in screen units.</returns>
        private Vector GetPieceTopLeftInScreenUnits(PuzzlePiece piece)
        {
            Vector pieceOffset = GetPieceTopLeftInPieceUnits(piece);
            return new Vector(Math.Max(0, edgeLength * (pieceOffset.X - overlap)), Math.Max(0, edgeLength * (pieceOffset.Y - overlap)));
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Gets the offset from the top left of the puzzle to the center of a PuzzlePiece.
        /// </summary>
        /// <param name="piece">The piece.</param>
        /// <returns>The offset from the top left of the puzzle to the center of the piece, measured in screen units.</returns>
        private Vector GetRelativePieceCenterInScreenUnits(PuzzlePiece piece)
        {
            Vector PieceCenter = GetAbsolutePieceCenterInScreenUnits(piece);
            Vector PieceOffset = GetPieceTopLeftInScreenUnits(piece);
            return PieceOffset + PieceCenter;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Gets the offset from the top left of a PuzzlePiece to that same piece's center
        /// </summary>
        /// <param name="piece">The piece for which to calculate the center</param>
        /// <returns>The center of the piece, measured in screen units</returns>
        private static Vector GetAbsolutePieceCenterInScreenUnits(PuzzlePiece piece)
        {
            return new Vector(Math.Ceiling(piece.ClipShape.Bounds.Width) / 2, Math.Ceiling(piece.ClipShape.Bounds.Height) / 2);
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Gets the center of a specific piece number in a PuzzlePiece
        /// </summary>
        /// <param name="piece">The piece number for which to find the center</param>
        /// <param name="pieceNumber">The piece in which the piece number exists</param>
        /// <returns>The center of the piece numberm in screen units</returns>
        private Vector GetPieceNumberCenterInScreenUnits(PuzzlePiece piece, int pieceNumber)
        {
            Vector pieceOffset = GetPieceTopLeftInPieceUnits(piece);
            Vector pieceNumberOffset = new Vector(GetPieceColumn(pieceNumber), GetPieceRow(pieceNumber));
            Vector pieceCenter = pieceNumberOffset - pieceOffset;
            pieceCenter *= edgeLength;
            pieceCenter += halfEdge;

            if (pieceOffset.X != 0)
            {
                pieceCenter.X += overlap * edgeLength / 2;
            }
            if (pieceOffset.Y != 0)
            {
                pieceCenter.Y += overlap * edgeLength / 2;
            }

            return pieceCenter;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Gets the offset between the center of two pieces.
        /// </summary>
        /// <param name="piece1">The first of the two pieces.</param>
        /// <param name="piece2">The second of the two pieces.</param>
        /// <returns>The offset between the centers of the two pieces, measured in pieces.</returns>
        private Vector GetRelativeCenterOffsetInPieceUnits(PuzzlePiece piece1, PuzzlePiece piece2)
        {
            return GetRelativePieceCenterInScreenUnits(piece2) - GetRelativePieceCenterInScreenUnits(piece1);
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Determines if the angles of two pieces are close enough to perform a join.
        /// </summary>
        /// <param name="orientation1">The orientation of the first piece.</param>
        /// <param name="orientation2">The orientation of the second piece.</param>
        /// <returns>True if a join can be performed, false otherwise.</returns>
        private static bool AreOrientationsWithinThreshold(double orientation1, double orientation2)
        {
            // Adjust the orientations so that comparing items across the 0/360 threshold pass correctly (ie. 1 and 359 should pass)
            if (Math.Abs(orientation1 - orientation2) > 180)
            {
                orientation1 += orientation2 > orientation1 ? 360 : -360;
            }

            double difference = Math.Abs(Math.Abs(orientation1) - Math.Abs(orientation2));

            if (difference > orientationThreshold)
            {
                return false;
            }
            return true;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Determines if two pieces are close enough to perform a join.
        /// </summary>
        /// <param name="offset">A vector that represents the distance between two pieces.</param>
        /// <returns>True if a join can be performed, false otherwise.</returns>
        private static bool IsDistanceWithinThreshold(Vector offset)
        {
            double absoluteLength = Math.Abs(offset.Length);
            return (absoluteLength > lowerDistanceThreshold && absoluteLength < upperDistanceThreshold);
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Determines which direction (if any) is a valid join direction based 
        /// on the angle between two pieces and the angle threshold.
        /// </summary>
        /// <param name="angleBetween">The angle between two pieces.</param>
        /// <returns>The direction where a join can happen.</returns>
        private static Direction DetermineJoinDirection(double angleBetween)
        {
            // Is the item to the right?
            if ((angleBetween > (angleRight - orientationThreshold) && angleBetween < (angleRight + orientationThreshold)))
            {
                return Direction.Right;
            }

            // Is the item above?
            if ((angleBetween > (angleUp - orientationThreshold) && angleBetween < (angleUp + orientationThreshold)))
            {
                return Direction.Up;
            }

            // Is the item to the left?
            if ((angleBetween > (angleLeft - orientationThreshold) && angleBetween < (angleLeft + orientationThreshold)))
            {
                return Direction.Left;
            }

            // Is the item below?
            if ((angleBetween > (angleDown - orientationThreshold) && angleBetween < (angleDown + orientationThreshold)))
            {
                return Direction.Down;
            }

            return Direction.None;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Calculates the end final center value to use in a join animation.
        /// </summary>
        /// <param name="pieceStayingStill">The piece that the moving piece is moving towards.</param>
        /// <param name="pieceBeingAnimated">The piece that is moving in the animation.</param>
        /// <returns>The target center point for the animation.</returns>
        public Point CalculateJoinAnimationDestination(ScatterViewItem pieceStayingStill, ScatterViewItem pieceBeingAnimated)
        {
            // Determine which side of pieceRemaining piecebeingAnimated will be placed on
            Vector offset = GetRelativeCenterOffsetInPieceUnits((PuzzlePiece)pieceStayingStill.Content, (PuzzlePiece)pieceBeingAnimated.Content);

            // Round the offsets so there are no fractions of screen units
            offset.X = Math.Round(offset.X);
            offset.Y = Math.Round(offset.Y);

            // Rotate the offset so it is relative to pieceRemaining's orientation
            Vector rotated = Rotate(offset, pieceStayingStill.ActualOrientation);

            // Use Center instead of ActualCenter because: 
            //  - Center is guaranteed to be set during the piece's animation onto the ScatterView
            //  - Center is potentially more accurate than ActualCenter, since this method can be called
            //    after Center is modified, but before a layout pass can update ActualCenter

            return pieceStayingStill.Center + rotated;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Calculates the adjustment that must be made to a ScatterViewItem's center when 
        /// its content is replaced with differently sized content so that the piece does not
        /// appear to jump after two pieces are joined.
        /// </summary>
        /// <param name="originalPiece">The original content of the item.</param>
        /// <param name="newPiece">The content that will become the item's content.</param>
        /// <returns>The adjustment that should be applied to that item's center, relative to the puzzle piece.</returns>
        public Vector CalculateJoinCenterAdjustment(PuzzlePiece originalPiece, PuzzlePiece newPiece)
        {
            Vector originalOffset = GetRelativePieceCenterInScreenUnits(originalPiece);
            Vector newOffset = GetRelativePieceCenterInScreenUnits(newPiece);

            double adjustmentX = Math.Ceiling(newOffset.X - originalOffset.X);
            double adjustnemtY = Math.Ceiling(newOffset.Y - originalOffset.Y);

            return new Vector(adjustmentX, adjustnemtY);
        }

        #endregion

        #region Piece Joining

        //---------------------------------------------------------//
        /// <summary>
        /// Try to join two puzzle pieces together.
        /// </summary>
        /// <param name="piece1">The first of two pieces to try to join.</param>
        /// <param name="piece2">The second of two pieces to try to join.</param>
        /// <returns>The result of the join.</returns>
        public PuzzlePiece JoinPieces(PuzzlePiece piece1, PuzzlePiece piece2)
        {
            // Combine the viewboxes
            VisualBrush newBrush = new VisualBrush(piece1.ImageBrush.Visual);
            newBrush.Viewbox = Rect.Union(piece1.ImageBrush.Viewbox, piece2.ImageBrush.Viewbox);
            newBrush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;

            // Combine the pieces
            HashSet<int> newPieces = new HashSet<int>(piece1.Pieces);
            newPieces.UnionWith(piece2.Pieces);

            // Combine the geometries
            Geometry newGeometry = CombineGeometries(piece1, piece2);

            // Now make them into a piece
            return new PuzzlePiece(newGeometry, newBrush, newPieces);
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Combines two Geometries, offset by the specified amount.
        /// </summary>
        /// <param name="geometry1">The first geometry to combine.</param>
        /// <param name="offset1">The offset of the first geometry relative to the second geometry, specified in puzzle piece units.</param>
        /// <param name="geometry1">The second geometry to combine.</param>
        /// <param name="offset1">The offset of the second geometry relative to the first geometry, specified in puzzle piece units.</param>
        /// <returns>The combined geometry</returns>
        private Geometry CombineGeometries(PuzzlePiece piece1, PuzzlePiece piece2)
        {
            // Get the geometires that will be joined
            Geometry geometry1 = piece1.ClipShape;
            Geometry geometry2 = piece2.ClipShape;

            // Get the offset of each piece relative to the puzzle
            Vector offset1InPixels = GetPieceTopLeftInScreenUnits(piece1);
            Vector offset2InPixels = GetPieceTopLeftInScreenUnits(piece2);

            // Use the offsets determined above to find the offset of each piece relative to the other piece
            // Between the two offsets, only one offset will have a nonzero value for x and y. This could be 
            // the same offset, or different offsets. Possible combinations are (0,0) and (100, 100); (50, 0) 
            // and (0, 75) but not (10, 0) and (15, 0)
            Vector relativeOffset1 = new Vector(Math.Max(0, Math.Round(offset1InPixels.X - offset2InPixels.X)), Math.Max(0, Math.Round(offset1InPixels.Y - offset2InPixels.Y)));
            Vector relativeOffset2 = new Vector(Math.Max(0, Math.Round(offset2InPixels.X - offset1InPixels.X)), Math.Max(0, Math.Round(offset2InPixels.Y - offset1InPixels.Y)));

            // Translate the geometries according to their offsets so they don't overlap when they're joined
            geometry1.Transform = new TranslateTransform(relativeOffset1.X, relativeOffset1.Y);
            geometry2.Transform = new TranslateTransform(relativeOffset2.X, relativeOffset2.Y);

            // Make a new GeometryGroup
            GeometryGroup newGeometry = new GeometryGroup();
            newGeometry.FillRule = FillRule.Nonzero;

            // Add the Geometries to the group
            newGeometry.Children.Add(geometry1);
            newGeometry.Children.Add(geometry2);

            // Flatten the group and return it
            return newGeometry.GetFlattenedPathGeometry();
        }

        #endregion

        #region Math / Helper Methods

        //---------------------------------------------------------//
        /// <summary>
        /// Returns the index to the column where the piece belongs in the puzzle.
        /// </summary>
        /// <param name="i">The piece number.</param>
        /// <returns>The index.</returns>
        private int GetPieceRow (int i)
        {
            return i / columns;
        }

        /// <summary>
        /// Returns the index to the row where the piece belongs in the puzzle.
        /// </summary>
        /// <param name="i">The piece number.</param>
        /// <returns>The index.</returns>
        private int GetPieceColumn(int i)
        {
            return i % columns;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Constrains the orientation such that it is always between -50 and 310.
        /// </summary>
        /// <remarks>
        /// Why -50 and 310?  The orientation threshold is 30 degrees, which works fine for
        /// Up (90) Left (180) and Down (270) but not Right (0 and 360) to avoid checking two 
        /// different angles every time for right, anything over 310 is converted to a negative
        /// angle so it can be compared to 0. 310/-50 was chosen because it is not contained in 
        /// any valid ranges, so all angle calculations can be done with one single angle 
        /// measurement.
        /// </remarks>
        /// <param name="orientation">The orientation to constratin.</param>
        /// <returns>The constrained orientation.</returns>
        private static double ConstrainOrientation(double orientation)
        {
            orientation %= 360;
            
            if (orientation > 310)
            {
                orientation -= 360;
            }
            if (orientation < -50)
            {
                orientation += 360;
            }

            return orientation;
        }

        /// <summary>
        /// Rotates a vector around its origin.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="degrees">The number of degrees to rotate the vector.</param>
        /// <returns>The rotated vector.</returns>
        public static Vector Rotate(Vector vector, double degrees)
        {
            double radians = ToRadians(degrees);
            return new Vector(vector.X * Math.Cos(radians) - vector.Y * Math.Sin(radians),
                              vector.X * Math.Sin(radians) + vector.Y * Math.Cos(radians));
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Convert an angle measurement in degrees to radians.
        /// </summary>
        /// <param name="degrees">Angle measurement in degrees.</param>
        /// <returns>The equivalent measurement in radians.</returns>
        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        #endregion
    }
}
