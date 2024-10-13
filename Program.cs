using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    static void Main(){
        List<string> LEX = lexer(read("main.casc"));

        for (int i = 0; i < LEX.Count; i++){
            Console.WriteLine(LEX[i]);
        }
    }

    static string read(string FilePath){
        string fileContent = File.ReadAllText(FilePath);
        return fileContent;
    }

    static List<string> splitter(string code){
        List<string> split = new List<string>();
        code += "\n";
        int j = 0;
        string Current = "";
        for (int i = 0; i < code.Length; i++){
            if (code[i].ToString() != "\n"){
                Current += code[i];
            }
            else{
                split.Add(Current);
                Current = "";
                j++;
            }
        }   
        return split;
    }

    static int Occurs(string str, string substring){
        return (str.Length - str.Replace(substring, "").Length) / substring.Length;
    }

    static List<string> lexer(string code){
        List<string> tokens = new List<string>();
        List<string> newcode = splitter(code);
        
        for (int i = 0; i < newcode.Count; i++){
            int tabChars = Occurs(newcode[i], "    ");
            newcode[i] = newcode[i].Replace("    ", "");

            if (newcode[i].StartsWith("return ")){
                tokens.Add("TAB=" + tabChars.ToString() + ";RETURN;" + newcode[i][6..].Trim());
            }

            else if (newcode[i].StartsWith("if ")){
                tokens.Add("TAB=" + tabChars.ToString() + ";IF;" + newcode[i][3..].Trim());
            }

            else if (newcode[i].StartsWith("int ")){
                string currcode = newcode[i][4..];
                int Space = currcode.IndexOf(" ");
                tokens.Add("TAB=" + tabChars.ToString() + ";INT;" + "NAME=" + currcode[..Space].Trim() + ";VAL=" + currcode[(Space + 2)..].Trim());
            }

            else if (newcode[i].StartsWith("str ")){
                string currcode = newcode[i][4..];
                int Space = currcode.IndexOf(" ");
                tokens.Add("TAB=" + tabChars.ToString() + ";STR;" + "NAME=" + currcode[..Space].Trim() + ";VAL=" + currcode[(Space + 2)..].Trim());
            }

            else if (newcode[i].StartsWith("bool ")){
                string currcode = newcode[i][5..];
                int Space = currcode.IndexOf(" ");
                tokens.Add("TAB=" + tabChars.ToString() + ";BOOL;" + "NAME=" + currcode[..Space].Trim() + ";VAL=" + currcode[(Space + 2)..].Trim());
            }

            else if (newcode[i].StartsWith("list ")){
                string currcode = newcode[i][5..];
                int Space = currcode.IndexOf(" ");
                tokens.Add("TAB=" + tabChars.ToString() + ";LIST;" + "NAME=" + currcode[..Space].Trim() + ";VAL=" + currcode[(Space + 2)..].Trim());
            }

            else if (newcode[i].StartsWith("func ")){
                string currcode = newcode[i][5..];
                int Space = currcode.IndexOf(" ");
                tokens.Add("TAB=" + tabChars.ToString() + ";FUNC;" + "NAME=" + currcode[..Space].Trim() + ";VAL=" + currcode[(Space + 2)..].Trim());
            }
        }
        return tokens;

    }
    static string Compiler(List<string> Tokens){
        // int main(){
        // return 0;
        // }


        // global _start
        // _start:
        //     mov eax, 1
        //     mov ebx, 0
        //     push ebx
        //     call ExitProces
    }
}