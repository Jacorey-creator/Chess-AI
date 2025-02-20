using NUnit.Framework;
using Moq;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Chess.Game;
using Chess;
using static Chess.Game.GameManager;

namespace Assets.Scripts.Testing.Chess960Tests
{
    class Chess960Testing
    {
        [Test]
        public void Test_Toggle_Chess960()
        {
            GameManager gameManager = new();
            // Arrange
            bool initialState = gameManager.chess960;

            // Act
            gameManager.Chess960Game();

            // Assert
            Assert.That(gameManager.chess960, Is.Not.EqualTo(initialState), "chess960 mode should toggle.");
        }


        [Test]
        public void Test_GenerateChess960FEN_HappyPath()
        {
            // Arrange
            string fen = FenUtility.GenerateChess960FEN();
           
            // Act & Assert
            Assert.That(fen.Split('/').Length == 8, "FEN should have 8 ranks");
            Assert.That(fen.Contains("k") && fen.Contains("r") && fen.Contains("b"), "FEN should contain a king, rook, and bishop");
        }

        [Test]
        public void Test_FEN_ShouldNotBeNull()
        {
            // Act
            string fen = FenUtility.GenerateChess960FEN();

            // Assert
            Assert.That(fen, Is.Not.Null, "FEN string should not be null");
        }
        [Test]
        public void Test_Chess960Game_ShouldCallNewGame()
        {
            // Arrange
            var mockGameManager = new Mock<GameManager>();
            bool newGameCalled = false;

            // Set up the mock to track when NewGame is called
            mockGameManager.Setup(gm => gm.NewGame(It.IsAny<bool>())).Callback<bool>((humanPlaysWhite) =>
            {
                newGameCalled = true;
            });

            // Act
            mockGameManager.Object.Chess960Game();

            // Assert
            Assert.That(newGameCalled, Is.True, "NewGame should be called after toggling chess960 mode");
        }

    }

}
