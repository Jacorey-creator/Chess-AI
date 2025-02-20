﻿namespace Chess {
	using System;
	using System.Collections.Generic;
    using System.Linq;

    public static class FenUtility {

		static Dictionary<char, int> pieceTypeFromSymbol = new Dictionary<char, int> () {
			['k'] = Piece.King, ['p'] = Piece.Pawn, ['n'] = Piece.Knight, ['b'] = Piece.Bishop, ['r'] = Piece.Rook, ['q'] = Piece.Queen
		};

		public const string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        // Generate a random Chess960 FEN string
        public static string GenerateChess960FEN()
        {
            char[] backRank = new char[8] { 'R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R' };

            // Shuffle the pieces while following Chess960 rules
            ShuffleBackRank(backRank);

            // Convert the rank into a FEN string
            string blackRank = new string(backRank);
            string whiteRank = blackRank.ToLower();

            // Standard Chess960 pawn and board structure
            return $"{whiteRank}/pppppppp/8/8/8/8/PPPPPPPP/{blackRank} w KQkq - 0 1";
        }

        private static void ShuffleBackRank(char[] backRank)
        {
            Random random = new Random();

            // Ensure bishops are on opposite-colored squares
            int lightBishopIndex = random.Next(0, 4) * 2; // Even index (light square)
            int darkBishopIndex = random.Next(0, 4) * 2 + 1; // Odd index (dark square)
            backRank[lightBishopIndex] = 'B';
            backRank[darkBishopIndex] = 'B';

            // Remaining positions after bishops are placed
            List<int> availablePositions = Enumerable.Range(0, 8)
                .Where(i => i != lightBishopIndex && i != darkBishopIndex)
                .ToList();

            ShuffleList(availablePositions, random);

            // Assign remaining pieces: Queen, Knights, Rooks, King
            backRank[availablePositions[0]] = 'Q';
            backRank[availablePositions[1]] = 'N';
            backRank[availablePositions[2]] = 'N';
            backRank[availablePositions[3]] = 'R';
            backRank[availablePositions[4]] = 'K';
            backRank[availablePositions[5]] = 'R';

            // Ensure the king is between the rooks
            while (!(Array.IndexOf(backRank, 'R') < Array.IndexOf(backRank, 'K') &&
                     Array.LastIndexOf(backRank, 'R') > Array.IndexOf(backRank, 'K')))
            {
                ShuffleList(availablePositions, random);
                backRank[availablePositions[3]] = 'R';
                backRank[availablePositions[4]] = 'K';
                backRank[availablePositions[5]] = 'R';
            }
        }
        private static void ShuffleList(List<int> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // Load position from fen string
        public static LoadedPositionInfo PositionFromFen (string fen) {

			LoadedPositionInfo loadedPositionInfo = new LoadedPositionInfo ();
			string[] sections = fen.Split (' ');

			int file = 0;
			int rank = 7;

			foreach (char symbol in sections[0]) {
				if (symbol == '/') {
					file = 0;
					rank--;
				} else {
					if (char.IsDigit (symbol)) {
						file += (int) char.GetNumericValue (symbol);
					} else {
						int pieceColour = (char.IsUpper (symbol)) ? Piece.White : Piece.Black;
						int pieceType = pieceTypeFromSymbol[char.ToLower (symbol)];
						loadedPositionInfo.squares[rank * 8 + file] = pieceType | pieceColour;
						file++;
					}
				}
			}

			loadedPositionInfo.whiteToMove = (sections[1] == "w");

			string castlingRights = (sections.Length > 2) ? sections[2] : "KQkq";
			loadedPositionInfo.whiteCastleKingside = castlingRights.Contains ("K");
			loadedPositionInfo.whiteCastleQueenside = castlingRights.Contains ("Q");
			loadedPositionInfo.blackCastleKingside = castlingRights.Contains ("k");
			loadedPositionInfo.blackCastleQueenside = castlingRights.Contains ("q");

			if (sections.Length > 3) {
				string enPassantFileName = sections[3][0].ToString ();
				if (BoardRepresentation.fileNames.Contains (enPassantFileName)) {
					loadedPositionInfo.epFile = BoardRepresentation.fileNames.IndexOf (enPassantFileName) + 1;
				}
			}

			// Half-move clock
			if (sections.Length > 4) {
				int.TryParse (sections[4], out loadedPositionInfo.plyCount);
			}
			return loadedPositionInfo;
		}

		// Get the fen string of the current position
		public static string CurrentFen (Board board) {
			string fen = "";
			for (int rank = 7; rank >= 0; rank--) {
				int numEmptyFiles = 0;
				for (int file = 0; file < 8; file++) {
					int i = rank * 8 + file;
					int piece = board.Square[i];
					if (piece != 0) {
						if (numEmptyFiles != 0) {
							fen += numEmptyFiles;
							numEmptyFiles = 0;
						}
						bool isBlack = Piece.IsColour (piece, Piece.Black);
						int pieceType = Piece.PieceType (piece);
						char pieceChar = ' ';
						switch (pieceType) {
							case Piece.Rook:
								pieceChar = 'R';
								break;
							case Piece.Knight:
								pieceChar = 'N';
								break;
							case Piece.Bishop:
								pieceChar = 'B';
								break;
							case Piece.Queen:
								pieceChar = 'Q';
								break;
							case Piece.King:
								pieceChar = 'K';
								break;
							case Piece.Pawn:
								pieceChar = 'P';
								break;
						}
						fen += (isBlack) ? pieceChar.ToString ().ToLower () : pieceChar.ToString ();
					} else {
						numEmptyFiles++;
					}

				}
				if (numEmptyFiles != 0) {
					fen += numEmptyFiles;
				}
				if (rank != 0) {
					fen += '/';
				}
			}

			// Side to move
			fen += ' ';
			fen += (board.WhiteToMove) ? 'w' : 'b';

			// Castling
			bool whiteKingside = (board.currentGameState & 1) == 1;
			bool whiteQueenside = (board.currentGameState >> 1 & 1) == 1;
			bool blackKingside = (board.currentGameState >> 2 & 1) == 1;
			bool blackQueenside = (board.currentGameState >> 3 & 1) == 1;
			fen += ' ';
			fen += (whiteKingside) ? "K" : "";
			fen += (whiteQueenside) ? "Q" : "";
			fen += (blackKingside) ? "k" : "";
			fen += (blackQueenside) ? "q" : "";
			fen += ((board.currentGameState & 15) == 0) ? "-" : "";

			// En-passant
			fen += ' ';
			int epFile = (int) (board.currentGameState >> 4) & 15;
			if (epFile == 0) {
				fen += '-';
			} else {
				string fileName = BoardRepresentation.fileNames[epFile - 1].ToString ();
				int epRank = (board.WhiteToMove) ? 6 : 3;
				fen += fileName + epRank;
			}

			// 50 move counter
			fen += ' ';
			fen += board.fiftyMoveCounter;

			// Full-move count (should be one at start, and increase after each move by black)
			fen += ' ';
			fen += (board.plyCount / 2) + 1;

			return fen;
		}

		public class LoadedPositionInfo {
			public int[] squares;
			public bool whiteCastleKingside;
			public bool whiteCastleQueenside;
			public bool blackCastleKingside;
			public bool blackCastleQueenside;
			public int epFile;
			public bool whiteToMove;
			public int plyCount;

			public LoadedPositionInfo () {
				squares = new int[64];
			}
		}
	}
}