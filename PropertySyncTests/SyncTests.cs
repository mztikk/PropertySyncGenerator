using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PropertySyncTests
{
    [TestClass]
    public class SyncTests
    {
        [DataTestMethod]
        [DataRow("Max", "Mustermann")]
        public void SyncStringPerson(string firstName, string lastName)
        {
            var personModel = new PersonModel { FirstName = firstName, LastName = lastName };
            var personViewModel = new PersonViewModel();
            PropertySync.Sync(personModel, personViewModel);
            Assert.AreEqual(personModel.FirstName, personViewModel.FirstName);
            Assert.AreEqual(personModel.LastName, personViewModel.LastName);
        }

        [DataTestMethod]
        [DataRow("Max", "Mustermann")]
        public void SyncStringDiff(string firstName, string lastName)
        {
            var personModel = new PersonModel { FirstName = firstName, LastName = lastName };
            var c = new MyClass();
            PropertySync.Sync(personModel, c);
            Assert.AreNotEqual(personModel.FirstName, c.StringProperty);
        }
    }
}
