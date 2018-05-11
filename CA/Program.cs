using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CA
{
    public static class Program {

        static void Main(string[] args) {
            (new CellularAutomaton("aaabbbccc")).Run(true);
            
            // Generated complying words
            Console.WriteLine("WORDS BELONGING TO aNbNcN :");
            int[] n = { 1, 3, 6 };
            for (int i=0; i < n.Length; ++i) {
                var word = Multiply("a", n[i]) + Multiply("b", n[i]) + Multiply("c", n[i]);
                Console.WriteLine($"n={n[i]} \nInput word: {word}");
                var ca = new CellularAutomaton(word);
                ca.Run(true);
            }

            // Erroneous words
            Console.Write("ERRONOUS WORDS :");
            string[] erroneous = { "abbc", "cab", "baca", "hoop" };
            for (int i = 0; i < erroneous.Length; ++i) {
                Console.WriteLine($"\nInput word: {erroneous[i]}");
                var ca = new CellularAutomaton(erroneous[i]);
                ca.Run(true);
            }
            
        }

        public static string Multiply(this string source, int multiplier) {
            StringBuilder sb = new StringBuilder(multiplier * source.Length);
            for (int i = 0; i < multiplier; i++) { sb.Append(source); }
            return sb.ToString();
        }

    }

    class CellularAutomaton {
        string word;
        string[] automaton;
        string[] new_automaton;
        bool done;
        string output;
        bool result;

        Regex aCount = new Regex(@"(?<num>\d+)a"); 
        Regex cCount = new Regex(@"c(?<num>\d+)");
        Regex bCountLeft = new Regex(@"(?<num>\d+)b");
        Regex bCountRight = new Regex(@"b(?<num>\d+)");
        Regex ATotal = new Regex(@"A(?<num>\d+)");
        Regex BTotal = new Regex(@"B(?<num>\d+)");
        Regex CTotal = new Regex(@"C(?<num>\d+)");

        public CellularAutomaton(string word) {
            this.done = false;
            this.result = false;
            this.output = "unrecognized";

            if (string.IsNullOrWhiteSpace(word)) {
                this.done = true;
                this.result = true;
                this.output = "success";
            }
            if (word.Length % 3 != 0) {
                this.done = true;
            }
            word.ToLower();
            this.automaton = new string[word.Length + 2];
            this.new_automaton = new string[automaton.Length];
            automaton[0] = automaton[word.Length + 1] = "-";
            for (int i = 1; i<= word.Length; ++i) {
                automaton[i] = word[i - 1].ToString();
            }
            this.word = word;
        }

        // F - fail symbol, one occurrence interrupts the run
        // S - success symbol
        // Returns 'success' when an automaton is reduced to a single S symbol
        public bool Run(bool showOutput = false) {
            if (showOutput) {
                Console.WriteLine("0. \t" + string.Join("   \t", automaton));
            }
            int i = 1;
            while(!done) {
                if (showOutput) {
                    Console.Write(i + ". \t");
                }
                Generation();
                if (showOutput) {
                    Console.WriteLine(string.Join("   \t", automaton));
                }
                ++i;
            }
            Console.WriteLine("Result:  " + output + '\n');
            return result;
        }

        private void Generation() {
            automaton.CopyTo(new_automaton, 0);
            for (int i = 1; i <= word.Length; ++i) {

                if (automaton[i] == "-") {
                    continue;
                }
                // a (only depends on a left-hand symbol)
                if (automaton[i] == "a") {
                    if (!(automaton[i + 1] == "a" || automaton[i + 1] == "ab" || automaton[i + 1] == "b")) {
                        new_automaton[i] = "F";
                        done = true;
                    }
                    // - |a| .. -> |1a|
                    else if (automaton[i - 1] == "-") {
                        new_automaton[i] = "1a";
                    }
                    // 1a |a| .. -> |2a|
                    else if (aCount.IsMatch(automaton[i - 1])) {
                        Int32.TryParse(aCount.Match(automaton[i - 1]).Result("${num}"), out int currentCount);
                        new_automaton[i] = ((currentCount + 1) + "a");
                    }
                }
                //ab
                else if (automaton[i] == "ab") {
                    // 1a |ab| ..
                    if (aCount.IsMatch(automaton[i - 1])) {
                        Int32.TryParse(aCount.Match(automaton[i - 1]).Result("${num}"), out int count);
                        new_automaton[i] = "A" + count;
                    }
                }
                //bc
                else if (automaton[i] == "bc") {
                    // .. |bc| c0
                    if (cCount.IsMatch(automaton[i + 1])) {
                        Int32.TryParse(cCount.Match(automaton[i + 1]).Result("${num}"), out int count);
                        new_automaton[i] = "C" + count;
                    }
                }
                // b
                else if (automaton[i] == "b") {
                    // a |b| c -> |B1|
                    if (automaton[i - 1] == "a" && automaton[i + 1] == "c") {
                        new_automaton[i] = "B1";
                    }
                    // a |b| .. -> |ab|
                    else if (automaton[i - 1] == "a") {
                        new_automaton[i] = "ab";
                    }
                    // ab |b| bc -> |B3|
                    else if (automaton[i - 1] == "ab" && automaton[i + 1] == "bc") {
                        new_automaton[i] = "B3";
                    }
                    // 1b |b| b1 -> |B5|
                    else if (automaton[i - 1] == "ab" && automaton[i + 1] == "bc") {
                        int totalCount = 3;
                        Int32.TryParse(bCountLeft.Match(automaton[i - 1]).Result("${num}"), out int leftCount);
                        Int32.TryParse(bCountRight.Match(automaton[i - 1]).Result("${num}"), out int rightCount);
                        totalCount += (leftCount + rightCount);
                        new_automaton[i] = ("B" + totalCount);
                    }
                    // .. |b| c -> |bc|
                    else if (automaton[i + 1] == "c") {
                        new_automaton[i] = "bc";
                    }
                    // ab |b| .. -> |1b|
                    else if (automaton[i - 1] == "ab") {
                        new_automaton[i] = "1b";
                    }
                    // 1b |b| .. -> |2b|
                    else if (bCountLeft.IsMatch(automaton[i - 1])) {
                        Int32.TryParse(bCountLeft.Match(automaton[i - 1]).Result("${num}"), out int currentCount);
                        new_automaton[i] = ((currentCount + 1) + "b");
                    }
                    // .. |b| bc -> |b1|
                    else if (automaton[i + 1] == "bc") {
                        new_automaton[i] = "b1";
                    }
                    // .. |b| b1
                    else if (bCountRight.IsMatch(automaton[i + 1])) {
                        Int32.TryParse(bCountRight.Match(automaton[i + 1]).Result("${num}"), out int currentCount);
                        new_automaton[i] = ("b" + (currentCount + 1));
                    }
                    // - |b| .. / .. |b| - -> |F|
                    else if (automaton[i - 1] == "-" || automaton[i + 1] == "-") {
                        new_automaton[i] = "F";
                        done = true;
                    }
                }
                // c (only depends on a right-hand symbol)
                else if (automaton[i] == "c") {
                    if (!(automaton[i - 1] == "c" || automaton[i - 1] == "bc" || automaton[i - 1] == "b")) {
                        new_automaton[i] = "F";
                        done = true;
                    }
                    // .. |a| - -> |c1|
                    else if (automaton[i + 1] == "-") {
                        new_automaton[i] = "c1";
                    }
                    // .. |a| c1 -> |2a|
                    else if (cCount.IsMatch(automaton[i + 1])) {
                        Int32.TryParse(cCount.Match(automaton[i + 1]).Result("${num}"), out int currentCount);
                        new_automaton[i] = ("c" + (currentCount + 1));
                    }
                }
                // 0a, c0
                else if (aCount.IsMatch(automaton[i]) || cCount.IsMatch(automaton[i])) {
                    new_automaton[i] = "-";
                }
                // A0
                else if (ATotal.IsMatch(automaton[i])) {
                    // - |A0| B0 -> S/F
                    if (BTotal.IsMatch(automaton[i + 1])) {
                        Int32.TryParse(ATotal.Match(automaton[i]).Result("${num}"), out int myCount);
                        Int32.TryParse(BTotal.Match(automaton[i + 1]).Result("${num}"), out int neighborCount);
                        if (myCount == neighborCount) {
                            new_automaton[i] = "S";
                        }
                        else {
                            new_automaton[i] = "F";
                            done = true;
                        }
                    }
                    // - |A0| C0 -> S/F
                    else if (CTotal.IsMatch(automaton[i + 1])) {
                        Int32.TryParse(ATotal.Match(automaton[i]).Result("${num}"), out int myCount);
                        Int32.TryParse(CTotal.Match(automaton[i + 1]).Result("${num}"), out int neighborCount);
                        if (myCount == neighborCount) {
                            new_automaton[i] = "S";
                        }
                        else {
                            new_automaton[i] = "F";
                            done = true;
                        }
                    }
                    // - |A0| 0b ...
                    else if (bCountLeft.IsMatch(automaton[i + 1])) {
                        continue;
                    }
                    else {
                        new_automaton[i] = "F";
                        done = true;
                    }
                }
                // B0
                else if (BTotal.IsMatch(automaton[i])) {
                    // 0a |B0| c0 -> S/F 
                    if (aCount.IsMatch(automaton[i - 1]) && cCount.IsMatch(automaton[i + 1])) {
                        Int32.TryParse(BTotal.Match(automaton[i]).Result("${num}"), out int myCount);
                        Int32.TryParse(aCount.Match(automaton[i - 1]).Result("${num}"), out int leftCount);
                        Int32.TryParse(cCount.Match(automaton[i + 1]).Result("${num}"), out int rightCount);
                        if (leftCount == myCount && myCount == rightCount) {
                            new_automaton[i] = "S";
                        }
                        else {
                            new_automaton[i] = "F";
                            done = true;
                        }
                    }
                    // ab |B0| .. / .. |B0| bc ...
                    else if (automaton[i - 1] == "ab" || automaton[i + 1] == "bc") {
                        continue;
                    }
                    else {
                        new_automaton[i] = "-";
                    }
                }
                // C0
                else if (CTotal.IsMatch(automaton[i])) {
                    // B0 |C0| - -> S/F
                    if (BTotal.IsMatch(automaton[i - 1])) {
                        Int32.TryParse(CTotal.Match(automaton[i]).Result("${num}"), out int myCount);
                        Int32.TryParse(BTotal.Match(automaton[i - 1]).Result("${num}"), out int neighborCount);
                        if (myCount == neighborCount) {
                            new_automaton[i] = "S";
                        }
                        else {
                            new_automaton[i] = "F";
                        }
                    }
                    // A0 |C0| - -> S/F
                    else if (ATotal.IsMatch(automaton[i - 1])) {
                        Int32.TryParse(CTotal.Match(automaton[i]).Result("${num}"), out int myCount);
                        Int32.TryParse(ATotal.Match(automaton[i - 1]).Result("${num}"), out int neighborCount);
                        if (myCount == neighborCount) {
                            new_automaton[i] = "S";
                        }
                        else {
                            new_automaton[i] = "F";
                            done = true;
                        }
                    }
                    // b0 |C0| - ...
                    else if (bCountRight.IsMatch(automaton[i - 1])) {
                        continue;
                    }
                    else {
                        new_automaton[i] = "F";
                        done = true;
                    }
                }
                // 0b
                else if (bCountLeft.IsMatch(automaton[i])) {
                    // .. |0b| B0 -> |B0|
                    if (BTotal.IsMatch(automaton[i + 1])) {
                        new_automaton[i] = automaton[i + 1];
                    }
                    // .. |1b| b1 -> |B4|
                    else if (bCountRight.IsMatch(automaton[i + 1])) {
                        Int32.TryParse(bCountLeft.Match(automaton[i]).Result("${num}"), out int myCount);
                        Int32.TryParse(bCountRight.Match(automaton[i + 1]).Result("${num}"), out int neighborCount);
                        new_automaton[i] = "B" + (myCount + neighborCount + 2);
                    }
                }
                // b0
                else if (bCountRight.IsMatch(automaton[i])) {
                    // B0 |b0| .. -> |B0|
                    if (BTotal.IsMatch(automaton[i - 1])) {
                        new_automaton[i] = automaton[i - 1];
                    }
                    // 1b |b1| .. -> |B4|
                    else if (bCountLeft.IsMatch(automaton[i - 1])) {
                        Int32.TryParse(bCountRight.Match(automaton[i]).Result("${num}"), out int myCount);
                        Int32.TryParse(bCountLeft.Match(automaton[i - 1]).Result("${num}"), out int neighborCount);
                        new_automaton[i] = "B" + (myCount + neighborCount + 2);
                    }
                }
                // S (+ overall success check)
                else if (automaton[i] == "S") {
                    if (automaton[i - 1] == "-" || automaton[i + 1] == "-") {
                        new_automaton[i] = "-";
                        output = "success";
                        result = true;
                        done = true;
                    }
                }
            }

            // Fail condition: new generation is the same
            if (new_automaton.SequenceEqual(automaton)) {
                done = true;
                for (int i = 1; i < automaton.Length; ++i) {
                    if (new_automaton[i] != "-") {
                        new_automaton[i] = "F";
                    }
                }
            }

            new_automaton.CopyTo(automaton, 0);
        }

    }

}
