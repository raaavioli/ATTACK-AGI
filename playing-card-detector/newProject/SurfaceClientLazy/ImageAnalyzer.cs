using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.Cv;
using System.Drawing;

namespace SurfaceClient {
	class ImageAnalyzer {
		private Rectangle[] squares;
		private const int SQUARE_SIZE = 180; // 1080 / 6 = 180
		
		// Constructs an ImageAnalyzer, setting up the rectangles for the sub-images.
		public ImageAnalyzer() {
			squares = new Rectangle[12];
			for (int i = 0; i < 12; ++i) {
				// Offset square in x depending on the side of the screen.
				int screenSideOffset = i < 6 ? 0 : 1740; // 1920 - SQUARE_SIZE = 1740;
				squares[i] = new Rectangle(screenSideOffset, i * SQUARE_SIZE, SQUARE_SIZE, SQUARE_SIZE);
			}
		}

		// Takes a full-size image of the SUR40 screen and returns the string that represents the positioning of the cards.
		// The returned string is formatted as follows: "{player}:{position}:{suit}:{rank},{player}:{position}:{suit}:{rank},..."
		// Where player is 1 for the left-side player and 2 is for the right-side player,
		// position is a number 1-6 where 1 is the position at the top of the screen and 6 is the position at the bottom,
		// suit is S for spades, C for clubs, D for diamonds and H for hearts,
		// rank is a number 1-13 where 1 is an ace and 13 is a king.
		public string AnalyzeImage(byte[] imageData) {
			Image<Gray, byte> image = new Image<Gray, byte>(1920, 1080);
			image.Bytes = imageData;
			Image<Gray, byte>[] subImages = SplitImage(image);
			StringBuilder stringBuilder = new StringBuilder();

			for (int i = 0; i < 12; ++i) {
				int player = i < 6 ? 1 : 2;
				int position = (i % 6) + 1;
				stringBuilder.Append(AnalyzeSubImage(subImages[i], player, position));
			}

			return stringBuilder.ToString();
		}

		// Splits a full-size image into 12 separate sub-images, one for each position.
		private Image<Gray, byte>[] SplitImage(Image<Gray, byte> fullImage) {
			Image<Gray, byte>[] result = new Image<Gray, byte>[12];
			for (int i = 0; i < squares.Length; ++i) {
				result[i] = fullImage.GetSubRect(squares[i]);
			}
			return result;
		}

		// Analyzes a single sub-image for the given player and position and returns the string for that particular sub-image.
		// If nothing is found, an empty string is returned.
		private string AnalyzeSubImage(Image<Gray, byte> subImage, int player, int position) {
			char suit = FindSuit(subImage);
			int rank = FindRank(subImage);
			return (suit.Equals('U') || rank == -1) ? "" : string.Format("{0}:{1}:{2}:{3},", player, position, suit, rank);
		}

		// Returns the suit seen in the sub-image or U if none is found.
		private char FindSuit(Image<Gray, byte> subImage) {
			return 'S';
		}

		// Returns the rank seen in the sub-image, or -1 if none is found.
		private int FindRank(Image<Gray, byte> subImage) {
			return 1;
		}
	}
}
