using ViewModel;
using Xunit;
using Moq;


namespace ViewModel_Tests
{
    public class ViewModel_Tests
    {
        [Fact]
        public void DataFromCorrectFile()
        {
            string filename = @"..\\..\\..\\..\\TestFiles\\ViewModel_correct_file_test.txt";

            var errorSender = new Mock<IErrorSender>();
            var fileDialog = new Mock<IFileDialog>();

            fileDialog.Setup(x => x.OpenFileDialog()).Returns(filename);
            var testViewData = new ViewData(errorSender.Object, fileDialog.Object);

            Assert.True(testViewData.DataFromFile_Command.CanExecute(null));

            testViewData.DataFromFile_Command.Execute(null);
            errorSender.Verify(x => x.SendError(It.IsAny<string>()), Times.Never);

            Assert.NotNull(testViewData.DataSpline);
            Assert.NotNull(testViewData.DataPlot);
        }

        [Fact]
        public void DataFromIncorrectFile()
        {
            string filename = @"..\\..\\..\\..\\TestFiles\\ViewModel_incorrect_file_test.txt";

            var errorSender = new Mock<IErrorSender>();
            var fileDialog = new Mock<IFileDialog>();

            fileDialog.Setup(x => x.OpenFileDialog()).Returns(filename);
            var testViewData = new ViewData(errorSender.Object, fileDialog.Object);

            Assert.True(testViewData.DataFromFile_Command.CanExecute(null));

            testViewData.DataFromFile_Command.Execute(null);

            Assert.Null(testViewData.DataSpline);
            Assert.Null(testViewData.DataPlot);
        }
    }
}