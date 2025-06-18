namespace HRWebApp.Helper
{
    public static class TaxHelper
    {
        // Albanian tax rates
        private const decimal SOCIAL_SECURITY_RATE = 0.095m; // 9.5%
        private const decimal HEALTH_INSURANCE_RATE = 0.017m; // 1.7%

        public static decimal CalculateIncomeTax(decimal grossSalary)
        {
            if (grossSalary <= 40000m)
            {
                // 0 - 40,000 lek: 0% tax
                return 0;
            }
            else if (grossSalary >= 40001m && grossSalary <= 50000m)
            {
                // 40,001 - 50,000 lek: 50% * 13% of amount over 30,000
                var taxableAmount = Math.Max(0, grossSalary - 30000m);
                return Math.Round(taxableAmount * 0.13m * 0.5m, 0);
            }
            else if (grossSalary >= 50001m && grossSalary <= 200000m)
            {
                // 50,001 - 200,000 lek: 13% of amount over 30,000
                var taxableAmount = grossSalary - 30000m;
                return Math.Round(taxableAmount * 0.13m, 0);
            }
            else
            {
                // 200,001+ lek: 22,100 + 23% of amount over 200,000
                var baseAmount = 22100m; // Fixed amount for first 200,000
                var additionalAmount = (grossSalary - 200000m) * 0.23m;
                return Math.Round(baseAmount + additionalAmount, 0);
            }
        }

        public static decimal CalculateSocialSecurity(decimal grossSalary)
        {
            return Math.Round(grossSalary * SOCIAL_SECURITY_RATE, 0);
        }

        public static decimal CalculateHealthInsurance(decimal grossSalary)
        {
            return Math.Round(grossSalary * HEALTH_INSURANCE_RATE, 0);
        }

        public static (decimal socialSecurity, decimal healthInsurance, decimal incomeTax, decimal total) CalculateAllDeductions(decimal grossSalary)
        {
            var socialSecurity = CalculateSocialSecurity(grossSalary);
            var healthInsurance = CalculateHealthInsurance(grossSalary);
            var incomeTax = CalculateIncomeTax(grossSalary);
            var total = socialSecurity + healthInsurance + incomeTax;

            return (socialSecurity, healthInsurance, incomeTax, total);
        }

        public static decimal CalculateNetFromGross(decimal grossSalary)
        {
            var deductions = CalculateAllDeductions(grossSalary);
            return grossSalary - deductions.total;
        }

        // Reverse calculation: Given a target net salary, find the gross salary needed
        public static decimal CalculateGrossFromNet(decimal targetNetSalary)
        {
            // Use binary search for more accurate results
            decimal minGross = targetNetSalary;
            decimal maxGross = targetNetSalary * 2m;
            decimal tolerance = 1m; // 1 lek tolerance

            // First, find a suitable upper bound
            while (CalculateNetFromGross(maxGross) < targetNetSalary)
            {
                maxGross *= 2m;
            }

            // Binary search
            for (int i = 0; i < 50; i++)
            {
                decimal midGross = (minGross + maxGross) / 2m;
                decimal calculatedNet = CalculateNetFromGross(midGross);
                decimal difference = calculatedNet - targetNetSalary;

                if (Math.Abs(difference) <= tolerance)
                    return Math.Round(midGross, 0);

                if (calculatedNet > targetNetSalary)
                    maxGross = midGross;
                else
                    minGross = midGross;
            }

            return Math.Round((minGross + maxGross) / 2m, 0);
        }

        // Test method to verify calculations match the example
        public static void TestCalculation()
        {
            decimal testGross = 110000m;
            var deductions = CalculateAllDeductions(testGross);
            decimal calculatedNet = CalculateNetFromGross(testGross);

            // Results should match:
            // Social Security: 10,450
            // Health Insurance: 1,870  
            // Income Tax: 10,400
            // Net: 87,280
        }
    }
}