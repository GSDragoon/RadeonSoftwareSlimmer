using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NUnit.Framework;
using RadeonSoftwareSlimmer.Models.PreInstall;

namespace RadeonSoftwareSlimmer.Test.Models.PreInstall
{
    public class DisplayComponentListModelTest
    {
        [Test]
        public void LoadOrRefresh_Directory_Does_Not_Exist_Throws_DirectoryNotFoundException()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\test.txt", new MockFileData(string.Empty) }
            });

            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(fileSystem);

            Assert.That(() => { displayComponentListModel.LoadOrRefresh(@"C:\driver"); }, Throws.TypeOf<DirectoryNotFoundException>());
        }

        [Test]
        public void LoadOrRefresh_Missing_Files_List_Is_Empty()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.in_", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)) },
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver2.in", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test2{0}", Environment.NewLine)) },
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\inf.NotIt", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test3{0}", Environment.NewLine)) },
            });

            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(fileSystem);
            
            displayComponentListModel.LoadOrRefresh(@"C:\driver");
            
            List<DisplayComponentModel> displayComponentModels = new List<DisplayComponentModel>(displayComponentListModel.DisplayDriverComponents);
            Assert.That(displayComponentModels, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_Missing_Directory_List_Is_Empty()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\Packages\Drivers\Display\W116A_INF\component1\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)) },
                {@"C:\driver\Packages\Drivers\Audio\WT6A_INF\component1\driver2.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test2{0}", Environment.NewLine)) },
                {@"C:\driver\Packages\Apps\Display\WT6A_INF\component1\driver3.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test3{0}", Environment.NewLine)) },
            });

            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(fileSystem);

            displayComponentListModel.LoadOrRefresh(@"C:\driver");

            List<DisplayComponentModel> displayComponentModels = new List<DisplayComponentModel>(displayComponentListModel.DisplayDriverComponents);
            Assert.That(displayComponentModels, Is.Empty);
        }

        [Test]
        public void LoadOrRefresh_Single_Component_Returns_One_Component()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe", new MockFileData(string.Empty) },
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)) }
            });

            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(fileSystem);

            displayComponentListModel.LoadOrRefresh(@"C:\driver");
            
            List<DisplayComponentModel> displayComponentModels = new List<DisplayComponentModel>(displayComponentListModel.DisplayDriverComponents);
            Assert.That(displayComponentModels, Has.Count.EqualTo(1));
        }

        [Test]
        public void LoadOrRefresh_Double_Component_Returns_Two_Components()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe", new MockFileData(string.Empty) },
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)) },
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component2\driver.inf", new MockFileData(
                    string.Format("dummyline{0}dummyline2{0}[Strings]{0}desc\"test{0}", Environment.NewLine)) }
            });

            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(fileSystem);

            displayComponentListModel.LoadOrRefresh(@"C:\driver");

            List<DisplayComponentModel> displayComponentModels = new List<DisplayComponentModel>(displayComponentListModel.DisplayDriverComponents);
            Assert.That(displayComponentModels, Has.Count.EqualTo(2));
        }


        [Test]
        public void RemoveComponentsNotKeeping_Keep_True_Does_Not_Remove()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe", new MockFileData(string.Empty) },
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf", new MockFileData(string.Empty) }
            });

            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(fileSystem);
            displayComponentListModel.LoadOrRefresh(@"C:\driver");
            displayComponentListModel.DisplayDriverComponents.First().Keep = true;

            displayComponentListModel.RemoveComponentsNotKeeping();

            Assert.That(fileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\"), Is.True);
        }

        [Test]
        public void RemoveComponentsNotKeeping_Keep_False_Does_Remove()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe", new MockFileData(string.Empty) },
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf", new MockFileData(string.Empty) }
            });

            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(fileSystem);
            displayComponentListModel.LoadOrRefresh(@"C:\driver");
            displayComponentListModel.DisplayDriverComponents.First().Keep = false;

            displayComponentListModel.RemoveComponentsNotKeeping();

            Assert.That(fileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\"), Is.False);
        }

        [Test]
        public void RemoveComponentsNotKeeping_Can_Remove_Multiple_Components()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe", new MockFileData(string.Empty) },
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf", new MockFileData(string.Empty) },
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component2\driver.inf", new MockFileData(string.Empty) }
            });

            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(fileSystem);
            displayComponentListModel.LoadOrRefresh(@"C:\driver");
            foreach (DisplayComponentModel displayComponentModel in displayComponentListModel.DisplayDriverComponents)
            {
                displayComponentModel.Keep = false;
            }

            displayComponentListModel.RemoveComponentsNotKeeping();

            Assert.Multiple(() =>
            {
                Assert.That(fileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\"), Is.False);
                Assert.That(fileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component2\"), Is.False);
            });
        }

        [Test]
        public void RemoveComponentsNotKeeping_Multple_Remove_Only_Not_Kept()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\ccc2_install.exe", new MockFileData(string.Empty) },
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\driver.inf", new MockFileData(string.Empty) },
                {@"C:\driver\Packages\Drivers\Display\WT6A_INF\component2\driver.inf", new MockFileData(string.Empty) }
            });

            DisplayComponentListModel displayComponentListModel = new DisplayComponentListModel(fileSystem);
            displayComponentListModel.LoadOrRefresh(@"C:\driver");
            displayComponentListModel.DisplayDriverComponents.Last().Keep = false;

            displayComponentListModel.RemoveComponentsNotKeeping();

            Assert.Multiple(() =>
            {
                Assert.That(fileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component1\"), Is.True);
                Assert.That(fileSystem.Directory.Exists(@"C:\driver\Packages\Drivers\Display\WT6A_INF\component2\"), Is.False);
            });
        }
    }
}
