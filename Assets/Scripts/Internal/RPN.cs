using System;
using System.Collections.Generic;

public class RPN
{
    public static int EvaluateRPN(string expression, Dictionary<string, int> library)
    {

        Stack<int> stack = new Stack<int>();
        string[] tokens = expression.Split(' ');

        foreach (string token in tokens)
        {
            if (int.TryParse(token, out int number))
            {
                stack.Push(number);
            }
            else if (token == "base")
            {
                stack.Push(library["base"]);
            }
            else if (token == "wave")
            {
                stack.Push(library["wave"]);
            }
            else
            {
                int b = stack.Pop();
                int a = stack.Pop();

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
                    case "%": // I forgor
                        stack.Push(a % b);
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