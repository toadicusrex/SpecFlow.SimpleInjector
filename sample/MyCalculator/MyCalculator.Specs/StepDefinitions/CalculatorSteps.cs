using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace MyCalculator.Specs.StepDefinitions
{
    [Binding]
    public class CalculatorSteps
    {
        private readonly ICalculator _calculator;

        public CalculatorSteps(ICalculator calculator)
        {
            _calculator = calculator;
        }

        [Given(@"I have entered (.*) into the calculator")]
        public void GivenIHaveEnteredIntoTheCalculator(int operand)
        {
            _calculator.Enter(operand);
        }

        [Given(@"I have entered the following numbers")]
        public void GivenIHaveEnteredTheFollowingNumbers(Table table)
        {
            foreach (var number in table.Rows.Select(r => int.Parse(r["number"])))
            {
                _calculator.Enter(number);
            }
        }

        [When(@"I press add")]
        public void WhenIPressAdd()
        {
            _calculator.Add();
        }

        [When(@"I press multiply")]
        public void WhenIPressMultiply()
        {
            _calculator.Multiply();
        }

        [Then(@"the result should be (.*) on the screen")]
        public void ThenTheResultShouldBeOnTheScreen(int expectedResult)
        {
            Assert.AreEqual(expectedResult, _calculator.Result);
        }
    }
}
