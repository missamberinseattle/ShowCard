using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using ShowCard.Models;
using ShowCard.Services;
using Xunit;

namespace ShowCard.UnitTests.Services
{
    public class CardRunServciesUnitTests
    {
        private readonly ILogService _log;
        private readonly CardRunService _service;

        public CardRunServciesUnitTests()
        {
            _log = Substitute.For<ILogService>();
            _service = new CardRunService(_log);
        }

        private Performer BuildPerformer(int order, string name)
            => new Performer { RunOrder = order, Name = name };

        private Card BuildCard(string title)
            => new Card { Title = title, FaceImagePath = "face.png", BackImagePath = "back.png" };

        // ---------------------------------------------------------
        // POSITIVE TEST: ShuffleAndAssign assigns unique cards
        // ---------------------------------------------------------
        [Fact]
        public void ShuffleAndAssign_ShouldAssignUniqueCards_WhenEnoughCardsExist()
        {
            var performers = new List<Performer>
        {
            BuildPerformer(1, "A"),
            BuildPerformer(2, "B"),
            BuildPerformer(3, "C")
        };

            var suspects = new List<Card>
        {
            BuildCard("S1"), BuildCard("S2"), BuildCard("S3")
        };

            var weapons = new List<Card>
        {
            BuildCard("W1"), BuildCard("W2"), BuildCard("W3")
        };

            var locations = new List<Card>
        {
            BuildCard("L1"), BuildCard("L2"), BuildCard("L3")
        };

            _service.ShuffleAndAssign(performers, suspects, weapons, locations);

            performers.Should().OnlyContain(p =>
                p.Suspect != null &&
                p.Weapon != null &&
                p.Location != null);

            var duplicates = _service.FindDuplicateAssignedCards(performers);
            duplicates.Should().BeEmpty();
        }

        // ---------------------------------------------------------
        // NEGATIVE TEST: Not enough cards → duplicates expected
        // ---------------------------------------------------------
        [Fact]
        public void ShuffleAndAssign_ShouldWarn_WhenNotEnoughCardsExist()
        {
            var performers = new List<Performer>        
            {
                BuildPerformer(1, "A"),
                BuildPerformer(2, "B")
            };

            var suspects = new List<Card> { BuildCard("S1") };
            var weapons = new List<Card> { BuildCard("W1") };
            var locations = new List<Card> { BuildCard("L1") };

            _service.ShuffleAndAssign(performers, suspects, weapons, locations);

            _log.Received().Warn("Not enough suspect cards for all performers. Some performers will share suspects.");
            _log.Received().Warn("Not enough weapon cards for all performers. Some performers will share weapons.");
            _log.Received().Warn("Not enough location cards for all performers. Some performers will share locations.");

            var duplicates = _service.FindDuplicateAssignedCards(performers);
            duplicates.Should().NotBeEmpty();
        }

        // ---------------------------------------------------------
        // POSITIVE TEST: Duplicate detection returns empty list
        // ---------------------------------------------------------
        [Fact]
        public void FindDuplicateAssignedCards_ShouldReturnEmpty_WhenNoDuplicates()
        {
            var performers = new List<Performer>
        {
            new Performer
            {
                Name = "A",
                Suspect = BuildCard("S1"),
                Weapon = BuildCard("W1"),
                Location = BuildCard("L1")
            },
            new Performer
            {
                Name = "B",
                Suspect = BuildCard("S2"),
                Weapon = BuildCard("W2"),
                Location = BuildCard("L2")
            }
        };

            var result = _service.FindDuplicateAssignedCards(performers);

            result.Should().BeEmpty();
        }

        // ---------------------------------------------------------
        // NEGATIVE TEST: Duplicate detection finds duplicates
        // ---------------------------------------------------------
        [Fact]
        public void FindDuplicateAssignedCards_ShouldReturnDuplicates_WhenDuplicatesExist()
        {
            var duplicateCard = BuildCard("SAME");

            var performers = new List<Performer>
        {
            new Performer
            {
                Name = "A",
                Suspect = duplicateCard,
                Weapon = BuildCard("W1"),
                Location = BuildCard("L1")
            },
            new Performer
            {
                Name = "B",
                Suspect = duplicateCard,
                Weapon = BuildCard("W2"),
                Location = BuildCard("L2")
            }
        };

            var result = _service.FindDuplicateAssignedCards(performers);

            result.Should().ContainSingle()
                  .And.Contain("SAME");
        }

        // ---------------------------------------------------------
        // NEGATIVE TEST: Missing cards produce error-card entries
        // ---------------------------------------------------------
        [Fact]
        public void FindDuplicateAssignedCards_ShouldIncludeErrorCards_WhenAssignmentsAreNull()
        {
            var performers = new List<Performer>
        {
            new Performer { Name = "A", Suspect = null, Weapon = null, Location = null }
        };

            var result = _service.FindDuplicateAssignedCards(performers);

            result.Should().HaveCount(3);
            result.Should().Contain(r => r.Contains("Performer: A; Type: Suspect"));
            result.Should().Contain(r => r.Contains("Performer: A; Type: Weapon"));
            result.Should().Contain(r => r.Contains("Performer: A; Type: Location"));
        }

        // ---------------------------------------------------------
        // POSITIVE TEST: GetNextPerformer returns correct next performer
        // ---------------------------------------------------------
        [Fact]
        public void GetNextPerformer_ShouldReturnNextInOrder()
        {
            var performers = new List<Performer>
        {
            BuildPerformer(1, "A"),
            BuildPerformer(2, "B"),
            BuildPerformer(3, "C")
        };

            var next = _service.GetNextPerformer(performers, 1);

            next.Should().NotBeNull();
            next!.Name.Should().Be("B");
        }

        // ---------------------------------------------------------
        // NEGATIVE TEST: GetNextPerformer returns null when none exist
        // ---------------------------------------------------------
        [Fact]
        public void GetNextPerformer_ShouldReturnNull_WhenNoNextExists()
        {
            var performers = new List<Performer>
        {
            BuildPerformer(1, "A"),
            BuildPerformer(2, "B")
        };

            var next = _service.GetNextPerformer(performers, 2);

            next.Should().BeNull();
        }
    }
}