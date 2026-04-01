using System;

class Program
{
    static void Main()
    {
        int num1 = 5;
        int num2 = 10;
        int num3 = 15;
        int sum = AddNumbers(num1, num2, num3);
        
        Console.WriteLine($"The sum of {num1}, {num2}, and {num3} is: {sum}");
    }
    
    static int AddNumbers(int a, int b, int c)
    {
        return a + b + c;
    }
}
