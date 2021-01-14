using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PropertySyncTests
{
    [TestClass]
    public class SyncTests
    {
        [DataTestMethod]
        [DataRow("Max", "Mustermann")]
        public void SyncString(string firstName, string lastName)
        {
            var personModel = new PersonModel { FirstName = firstName, LastName = lastName };
            var personViewModel = new PersonViewModel();
            PropertySync.Sync(personModel, personViewModel);
            Assert.AreEqual(personModel.FirstName, personViewModel.FirstName);
            Assert.AreEqual(personModel.LastName, personViewModel.LastName);
        }
    }
}
