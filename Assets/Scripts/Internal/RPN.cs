using System;
using System.Collections.Generic;

public class RPN // Call using RPN.EvaluateRPN(*expression*);
{
    public static double EvaluateRPN(string expression)
    {

        Stack<double> stack = new Stack<double>();
        string[] tokens = expression.Split(' ');

        foreach (string token in tokens)
        {
            if (double.TryParse(token, out double number))
            {
                stack.Push(number);
            }
            else
            {
                double b = stack.Pop();
                double a = stack.Pop();

                switch (token)
                {
                    case "+":
                        stack.Push(a + b);
                        break;
                    case "-":
                        stack.Push(a - b);
                        break;
                    case "*":
                        stack.Push(a * b);
                        break;
                    case "/":
                        stack.Push(a / b);
                        break;
                    default:
                        throw new InvalidOperationException($"{token} not recognized. Try again.");
                }
            }
        }

        if (stack.Count != 1)
        {
            throw new InvalidOperationException("Invalid expression.");
        }

        return stack.Pop(); // Returns final value in stack instead of storing popped value in variable. Seemed mildly more efficient.
    }
}