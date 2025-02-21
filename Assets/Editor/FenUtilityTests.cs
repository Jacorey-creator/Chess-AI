using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Chess.Game;
using System.Runtime.InteropServices;
using System;
using System.Linq;

namespace Chess.Tests
{
    public class FenUtilityTests
    {
        [NUnit.Framework.Test]
        public void GenerateChess960FEN_CreatesValidFEN()
        {
            string fen = GenerateChess960FEN();
            string[] ranks = fen.Split('/');
            Assert.AreEqual(8, ranks.Length, "FEN must have 8 ranks");
            Assert.AreEqual("pppppppp", ranks[1], "Second rank must contain only pawns");
            Assert.AreEqual("PPPPPPPP", ranks[6], "Seventh rank must contain only pawns");
            Assert.AreEqual(" w KQkq - 0 1", fen.Substring(fen.IndexOf(' ')), "FEN must have correct game state info");
        }
        [Test]
        public void GenerateChess960FEN_ShouldNotThrowException()
        {
            // Wrap the method call in a try-catch block to verify no exception is thrown
            try
            {
                string fen = GenerateChess960FEN();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception thrown: {ex.Message}");
            }
        }
        [Test]
        public void GenerateChess960FEN_ProducesDifferentPositions()
        {
            string fen1 = FenUtility.startFen;
            string fen2 = GenerateChess960FEN();

            Assert.AreNotEqual(fen1, fen2, "Generated Chess960 FENs should not always be identical.");
        }
        [Test]
        public void ShuffleBackRank_CreatesValidPosition()
        {
            char[] backRank = new char[8];
            ShuffleBackRank(backRank);

            // Ensure correct piece count
            Assert.AreEqual(1, backRank.Count(c => c == 'K'), "There must be one King");
            Assert.AreEqual(1, backRank.Count(c => c == 'Q'), "There must be one Queen");
            Assert.AreEqual(2, backRank.Count(c => c == 'B'), "There must be two Bishops");
            Assert.AreEqual(2, backRank.Count(c => c == 'N'), "There must be two Knights");
            Assert.AreEqual(2, backRank.Count(c => c == 'R'), "There must be two Rooks");

            // Ensure bishops are on opposite colors
            int[] bishopIndices = backRank.Select((c, i) => new { c, i })
                                          .Where(x => x.c == 'B')
                                          .Select(x => x.i)
                                          .ToArray();
            Assert.AreEqual(2, bishopIndices.Length);
            Assert.AreNotEqual(bishopIndices[0] % 2, bishopIndices[1] % 2, "Bishops must be on opposite colors");

            // Ensure king is between the rooks
            int kingIndex = Array.IndexOf(backRank, 'K');
            int leftRookIndex = Array.IndexOf(backRank, 'R');
            int rightRookIndex = Array.LastIndexOf(backRank, 'R');
            Assert.IsTrue(leftRookIndex < kingIndex, "King must be to the right of the first Rook");
            Assert.IsTrue(rightRookIndex > kingIndex, "King must be to the left of the second Rook");
        }
        private void ShuffleBackRank(char[] backRank)
        {
            // Assume this is the function under test.
            FenUtility.ShuffleBackRank(backRank);
        }
        private string GenerateChess960FEN()
        {
            return FenUtility.GenerateChess960FEN();
        }
    }
}