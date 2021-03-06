﻿using System.IO;
using NBi.NUnit.Runtime;
using NBi.Xml;
using NUnit.Framework;
using System.Diagnostics;
using System.Reflection;
using NBi.Framework;
using System.Configuration;
using NBi.Core;
using System.Collections.Generic;

namespace NBi.Testing.Acceptance
{
    /// <summary>
    /// This class is the only one in the namespace "NBi.Testing.Acceptance" with a TestFixture.
    /// NUnit (more specifically the SimpleTestRunner created in method Run of class TestSuiteRunner) 
    /// will load this class as the entry point for Acceptance Test Suites.
    /// </summary>
    [TestFixture]
    public class RuntimeOverrider
    {

        //This class overrides the search for TestSuiteDefinitionFile
        //The filename is given by the TestCase here under
        public class TestSuiteOverrider : TestSuite
        {
            public TestSuiteOverrider(string filename)
                : this(filename, null)
            {
            }

            public TestSuiteOverrider(string filename, string configFilename) : base()
            {
                TestSuiteFinder = new TestSuiteFinderOverrider(filename);
                ConfigurationFinder = new ConfigurationFinderOverrider(configFilename);
                ConnectionStringsFinder = new ConnectionStringsFinderOverrider(configFilename);
            }

            internal class TestSuiteFinderOverrider : TestSuiteFinder
            {
                private readonly string filename;
                public TestSuiteFinderOverrider(string filename)
                {
                    this.filename = filename;
                }

                protected internal override string Find()
                {
                    return @"Acceptance\Resources\" + filename;
                }
            }

            internal class ConfigurationFinderOverrider : ConfigurationFinder
            {
                private readonly string filename;
                public ConfigurationFinderOverrider(string filename)
                {
                    this.filename = filename;
                }
                protected internal override NBiSection Find()
                {
                    if (!string.IsNullOrEmpty(filename))
                    {
                        var configuration = ConfigurationManager.OpenExeConfiguration(@"Acceptance\Resources\" + filename);

                        var section = (NBiSection)(configuration.GetSection("nbi"));
                        if (section != null)
                            return section;
                    }
                    return new NBiSection();
                }
            }

            internal class ConnectionStringsFinderOverrider : ConnectionStringsFinder
            {
                private readonly string filename;
                public ConnectionStringsFinderOverrider(string filename)
                {
                    this.filename = filename;
                }
                protected override string GetConfigFile() => $@"Acceptance\Resources\{filename}.config";

                protected override Configuration GetConfiguration()
                {
                    ExeConfigurationFileMap configMap = new ExeConfigurationFileMap()
                    {
                        ExeConfigFilename = GetConfigFile()
                    };
                    var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                    return config;
                }
            }

            [Ignore]
            public override void ExecuteTestCases(TestXml test)
            {
                base.ExecuteTestCases(test);
            }

            [Ignore]
            public void ExecuteTestCases(TestXml test, ITestConfiguration configuration)
            {
                base.Configuration = configuration;
                base.ExecuteTestCases(test);
            }
        }

        [TestFixtureSetUp]
        public void SetupMethods()
        {
            //Build the fullpath for the file to read
            Directory.CreateDirectory("Etl");
            DiskOnFile.CreatePhysicalFile(@"Etl\Sample.dtsx", "NBi.Testing.Integration.SqlServer.IntegrationService.Resources.Sample.dtsx");
        }

        //By Acceptance Test Suite (file) create a Test Case
        [Test]
        [TestCase("QueryUniqueRows.nbits")]
        [TestCase("AssemblyEqualToResultSet.nbits")]
        [TestCase("CsvEqualToResultSet.nbits")]
        [TestCase("QueryEqualToWithParameter.nbits")]
        [TestCase("QueryEqualToCsv.nbits")]
        [TestCase("QueryEqualToCsvWithProfile.nbits")]
        [TestCase("QueryEqualToQuery.nbits")]
        [TestCase("QuerySubsetOfQuery.nbits")]
        [TestCase("QuerySupersetOfQuery.nbits")]
        [TestCase("QueryEqualToResultSet.nbits")]
        [TestCase("QueryEqualToResultSetWithNull.nbits")]
        [TestCase("QueryWithReference.nbits")]
        [TestCase("Ordered.nbits")]
        [TestCase("Count.nbits")]
        [TestCase("Contain.nbits")]
        [TestCase("ContainStructure.nbits")]
        [TestCase("FasterThan.nbits")]
        [TestCase("SyntacticallyCorrect.nbits")]
        [TestCase("Exists.nbits")]
        [TestCase("LinkedTo.nbits")]
        [TestCase("SubsetOfStructure.nbits")]
        [TestCase("EquivalentToStructure.nbits")]
        [TestCase("SubsetOfMembers.nbits")]
        [TestCase("EquivalentToMembers.nbits")]
        [TestCase("MatchPatternMembers.nbits")]
        [TestCase("ResultSetMatchPattern.nbits")]
        [TestCase("QueryWithParameters.nbits")]
        [TestCase("EvaluateRows.nbits")]
        [TestCase("ReportEqualTo.nbits")]
        [TestCase("Etl.nbits")]
        [TestCase("Decoration.nbits")]
        [TestCase("Is.nbits")]
        [TestCase("QueryEqualToXml.nbits")]
        [TestCase("QueryRowCount.nbits")]
        [TestCase("QueryAllNoRows.nbits")]
        [TestCase("ResultSetConstraint.nbits")]
        //[TestCase("PowerBiDesktop.nbits")]
        [Category("Acceptance")]
        public void RunPositiveTestSuite(string filename)
        {
            var ignoredTests = new List<string>();
            var t = new TestSuiteOverrider(@"Positive\" + filename);

            //First retrieve the NUnit TestCases with base class (NBi.NUnit.Runtime)
            //These NUnit TestCases are defined in the Test Suite file
            var tests = t.GetTestCases();

            //Execute the NUnit TestCases one by one
            foreach (var testCaseData in tests)
            {
                try
                {
                    t.ExecuteTestCases((TestXml)testCaseData.Arguments[0]);
                }
                catch (IgnoreException)
                {
                    Trace.WriteLineIf(NBiTraceSwitch.TraceWarning, $"Not stopping the test suite, continue on ignore.");
                    ignoredTests.Add(((TestXml)testCaseData.Arguments[0]).Name);
                }
            }

            if (ignoredTests.Count>0)
                Assert.Inconclusive($"At least one test has been skipped. Check if it was expected. List of ignored tests: '{string.Join("', '", ignoredTests)}'");
        }

        [Test]
        [TestCase("QueryEqualToResultSetProvider.nbits")]
        [Category("Acceptance")]
        public void RunPositiveTestSuiteWithConfig(string filename)
        {
            var t = new TestSuiteOverrider(@"Positive\" + filename, @"Positive\" + filename);

            //First retrieve the NUnit TestCases with base class (NBi.NUnit.Runtime)
            //These NUnit TestCases are defined in the Test Suite file
            var tests = t.GetTestCases();

            //Execute the NUnit TestCases one by one
            foreach (var testCaseData in tests)
                t.ExecuteTestCases((TestXml)testCaseData.Arguments[0]);
        }

        [Test]
        [TestCase("DataRowsMessage.nbits")]
        [TestCase("ItemsMessage.nbits")]
        [Category("Acceptance")]
        public void RunNegativeTestSuite(string filename)
        {
            var t = new TestSuiteOverrider(@"Negative\" + filename);
            //These NUnit TestCases are defined in the Test Suite file
            var tests = t.GetTestCases();

            //Execute the NUnit TestCases one by one
            foreach (var testCaseData in tests)
            {
                var testXml = (TestXml)testCaseData.Arguments[0];
                try
                {
                    t.ExecuteTestCases(testXml);
                    Assert.Fail("The test named '{0}' (uid={1}) and defined in '{2}' should have failed but it hasn't."
                        , testXml.Name
                        , testXml.UniqueIdentifier
                        , filename);
                }
                catch (CustomStackTraceAssertionException ex)
                {
                    using (Stream stream = Assembly.GetExecutingAssembly()
                                           .GetManifestResourceStream(
                                                "NBi.Testing.Acceptance.Resources.Negative."
                                                + filename.Replace(".nbits", string.Empty)
                                                + "-" + testXml.UniqueIdentifier + ".txt"))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            //Debug.WriteLine(ex.Message);
                            Assert.That(ex.Message, Is.EqualTo(reader.ReadToEnd()));
                        }
                        Assert.That(ex.StackTrace, Is.Not.Null.Or.Empty);
                        Assert.That(ex.StackTrace, Is.EqualTo(testXml.Content));
                    }
                }
            }
        }

        [Test]
        [TestCase("Config-Full-Json.nbits")]
        [TestCase("Config-Full.nbits")]
        [TestCase("Config-Light.nbits")]
        [Category("Acceptance")]
        public void RunNegativeTestSuiteWithConfig(string filename)
        {
            var t = new TestSuiteOverrider(@"Negative\" + filename, @"Negative\" + filename);

            //First retrieve the NUnit TestCases with base class (NBi.NUnit.Runtime)
            //These NUnit TestCases are defined in the Test Suite file
            var tests = t.GetTestCases();

            //Execute the NUnit TestCases one by one
            foreach (var testCaseData in tests)
            {
                var testXml = (TestXml)testCaseData.Arguments[0];
                try
                {
                    t.ExecuteTestCases(testXml);
                    Assert.Fail("The test named '{0}' (uid={1}) and defined in '{2}' should have failed but it hasn't."
                        , testXml.Name
                        , testXml.UniqueIdentifier
                        , filename);
                }
                catch (CustomStackTraceAssertionException ex)
                {
                    using (Stream stream = Assembly.GetExecutingAssembly()
                                           .GetManifestResourceStream(
                                                "NBi.Testing.Acceptance.Resources.Negative."
                                                + filename.Replace(".nbits", string.Empty)
                                                + "-" + testXml.UniqueIdentifier + ".txt"))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            var expected = reader.ReadToEnd();
                            //Debug.WriteLine(expected);
                            //Debug.WriteLine("");
                            Debug.WriteLine(ex.Message);
                            Assert.That(ex.Message, Is.EqualTo(expected));
                        }
                        Assert.That(ex.StackTrace, Is.Not.Null.Or.Empty);
                        Assert.That(ex.StackTrace, Is.EqualTo(testXml.Content));
                    }
                }
            }
        }

        [Test]
        [TestCase("Ignored.nbits")]
        public void RunIgnoredTests(string filename)
        {
            var t = new TestSuiteOverrider(@"Ignored\" + filename);

            //First retrieve the NUnit TestCases with base class (NBi.NUnit.Runtime)
            //These NUnit TestCases are defined in the Test Suite file
            var tests = t.GetTestCases();

            //Execute the NUnit TestCases one by one
            foreach (var testCaseData in tests)
            {
                var isSuccess = false;
                try
                {
                    t.ExecuteTestCases((TestXml)testCaseData.Arguments[0]);
                }
                catch (IgnoreException)
                {
                    isSuccess = true;
                    Trace.WriteLineIf(NBiTraceSwitch.TraceVerbose, $"Expectation was met: test ignored.");
                }
                Assert.That(isSuccess);
            }
        }
    }
}
