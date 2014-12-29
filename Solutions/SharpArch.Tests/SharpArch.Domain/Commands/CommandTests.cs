namespace Tests.SharpArch.Domain.Commands
{
    using NUnit.Framework;

    [TestFixture]
    public class CommandTests
    {
        [Test]
        public void IsValidReturnsTrueForValidCommand()
        {
            var command = new ValidCommand();
            Assert.IsTrue(command.IsValid());
        }

        [Test]
        public void IsValidReturnsFalseForInvalidCommand()
        {
            var command = new InvalidCommand();
            Assert.IsFalse(command.IsValid());
        }

        [Test]
        public void CanGetValidationResultsForInvalidCommand()
        {
            var command = new InvalidCommand();
            var results = command.ValidationResults();

            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);

            var enumerator = results.GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual("The Invalid field is required.", enumerator.Current.ErrorMessage);

            enumerator.MoveNext();
            Assert.AreEqual("The field InvalidInt must be between 100 and 199.", enumerator.Current.ErrorMessage);
        }
    }
}