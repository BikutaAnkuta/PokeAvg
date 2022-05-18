using NUnit.Framework;

namespace PokeAvg.UnitTests
{
    public class PokeAvgTests
    {
        [Test]
        public void Capitalize_WordBecomesCapitalized_ReturnsTrue()
        {
            string lowerCaseString = "pokemon";

            string result = Program.Capitalize(lowerCaseString);

            Assert.IsTrue(result == "Pokemon");
        }

        [Test]
        public void CalculateAverage_TenItems_ReturnsTrue()
        {
            float total = 100f;
            int divisor = 10;

            var result = Program.CalculateAverage(total, divisor);

            Assert.IsTrue(result == 10);
        }

        [Test]
        public void CalculateAverage_TenItems_ReturnsFalse()
        {
            float total = 100f;
            int divisor = 20;

            var result = Program.CalculateAverage(total, divisor);

            Assert.IsFalse(result == 8);
        }
    }    
}