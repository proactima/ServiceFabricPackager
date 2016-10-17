using System;
using System.IO;
using FluentAssertions;
using SFPackager.Services;
using Xunit;

namespace SFPackager.Tests.DescribeSolutionParser
{
    public class DescribeExtractSolutions
    {
        [Fact]
        public void Stuff()
        {
            // g
            var parser = new SolutionParser();
            var solutionFile = new FileInfo(@"DescribeSolutionParser\TestSolution.sln");

            // w
            var actual = parser.ExtractSfProjects(solutionFile).GetAwaiter().GetResult();
            
            // t
            actual.Count.Should().Be(2);
            actual[0].Name.Should().Be("SfApp1.sfproj");
            actual[1].Name.Should().Be("SfApp2.sfproj");
        }
    }
}