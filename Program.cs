using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Globalization;

class Program
{
    static void Main(){

        string FilePath = "main.cil";

        //List<string> LEX = lexer(read(FilePath));

        //for (int i = 0; i < LEX.Count; i++){
        //    Console.WriteLine(LEX[i]);
        //}

        string Code = Compiler(lexer(read(FilePath)));

        string str = FilePath;
        char[] stringArray = str.ToCharArray();
        Array.Reverse(stringArray);
        string reversedPath = new string(stringArray);

        reversedPath = reversedPath[(reversedPath.IndexOf(".") + 1)..];
        char[] NewArray = reversedPath.ToCharArray();
        Array.Reverse(NewArray);
        string NewPath = new string(NewArray) + ".cpp";

        File.WriteAllText(NewPath, Code);
    }

    static string read(string FilePath){
        string fileContent = File.ReadAllText(FilePath);
        return fileContent;
    }

    static List<string> splitter(string code, string character){
        List<string> split = new List<string>();
        code += character;
        int j = 0;
        string Current = "";
        for (int i = 0; i < code.Length; i++){
            if (code[i].ToString() != character){
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

    static string GetImports(string code){
        List<string> Imports = new List<string>();
        List<string> newcode = splitter(code, "\n");

        for (int j = 0; j < newcode.Count; j++){
            if (newcode[j].StartsWith("import ")){
                Imports.Add(newcode[j][6..^2].Trim());
            }
        }

        string ImportedCode = "";

        for (int j = 0; j < Imports.Count; j++){
            ImportedCode += read(Imports[j]);
        }
        return ImportedCode;
    }

    static int Occurs(string str, string substring){
        return (str.Length - str.Replace(substring, "").Length) / substring.Length;
    }

    static string RemoveComments(string code){
        if (code.Length > code.Replace("; ", "").Length){
            return code[..code.IndexOf("; ")] + ";";
        }
        return code;
    }

    static List<string> lexer(string code){
        code = GetImports(code) + "\n" + code;

        List<string> tokens = new List<string>();
        List<string> newcode = splitter(code, "\n");
        int j = -1;
        
        for (int i = 0; i < newcode.Count; i++){
            if (newcode[i].Length > 1){
                int tabChars = Occurs(newcode[i], "    ");
                newcode[i] = newcode[i].Replace("    ", "");

                if (newcode[i].StartsWith("return ")){
                    j++;
                    tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";RETURN;" + newcode[i][6..].Trim());
                }

                else if (newcode[i].StartsWith("if ")){
                    j++;
                    tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";IF;COND=" + newcode[i][3..].Trim());
                    tokens[j] = RemoveComments(tokens[j]);
                }

                else if (newcode[i].StartsWith("int ")){
                    j++;
                    string currcode = newcode[i][4..];
                    int Space = currcode.IndexOf(' ');

                    tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";INT;NAME=" + currcode[..Space].Trim() + ";VAL=" + currcode[(Space + 2)..].Trim());
                    tokens[j] = RemoveComments(tokens[j]);
                }

                else if (newcode[i].StartsWith("str ")){
                    j++;
                    string currcode = newcode[i][4..];
                    int Space = currcode.IndexOf(' ');

                    tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";STR;NAME=" + currcode[..Space].Trim() + ";VAL=" + currcode[(Space + 2)..].Trim());
                    tokens[j] = RemoveComments(tokens[j]);
                }

                else if (newcode[i].StartsWith("bool ")){
                    j++;
                    string currcode = newcode[i][5..];
                    int Space = currcode.IndexOf(' ');

                    tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";BOOL;NAME=" + currcode[..Space].Trim() + ";VAL=" + currcode[(Space + 2)..].Trim());
                    tokens[j] = RemoveComments(tokens[j]);
                }

                else if (newcode[i].StartsWith("str[")){
                    j++;
                    string Size = newcode[i][(newcode[i].IndexOf("[") + 1)..newcode[i].IndexOf("]")];

                    newcode[i] = newcode[i][newcode[i].IndexOf("] ")..];
                    string currcode = newcode[i][2..];

                    int Space = currcode.IndexOf(' ');

                    tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";LIST;TYPE=STR;NAME=" + currcode[..Space].Trim() + ";VAL=" + currcode[(Space + 2)..^2].Trim() + ";SIZE=" + Size + ";");
                    tokens[j] = RemoveComments(tokens[j]);
                }

                else if (newcode[i].StartsWith("int[")){
                    j++;
                    string Size = newcode[i][(newcode[i].IndexOf("[") + 1)..newcode[i].IndexOf("]")];

                    newcode[i] = newcode[i][newcode[i].IndexOf("] ")..];
                    string currcode = newcode[i][2..];

                    int Space = currcode.IndexOf(' ');

                    tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";LIST;TYPE=INT;NAME=" + currcode[..Space].Trim() + ";VAL=" + currcode[(Space + 2)..^2].Trim() + ";SIZE=" + Size + ";");
                    tokens[j] = RemoveComments(tokens[j]);
                }

                else if (newcode[i].StartsWith("bool[")){
                    j++;
                    string Size = newcode[i][(newcode[i].IndexOf("[") + 1)..newcode[i].IndexOf("]")];

                    newcode[i] = newcode[i][newcode[i].IndexOf("] ")..];
                    string currcode = newcode[i][2..];

                    int Space = currcode.IndexOf(' ');

                    tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";LIST;TYPE=BOOL;NAME=" + currcode[..Space].Trim() + ";VAL=" + currcode[(Space + 2)..^2].Trim() + ";SIZE=" + Size + ";");
                    tokens[j] = RemoveComments(tokens[j]);
                }

                else if (newcode[i].StartsWith("func ")){
                    j++;
                    int ValStart = newcode[i].IndexOf('(');
                    char[] CodeChar = newcode[i].ToCharArray();
                    Array.Reverse(CodeChar);
                    int ValEnd = newcode[i].Length - new string(CodeChar).IndexOf(')');
                    string Name = newcode[i][5..ValStart].Trim();

                    string type = Name[..4].ToUpper();
                    if (!(type == "BOOL" || type == "VOID")){
                        type = Name[..3].ToUpper();
                        Name = Name[4..];
                    }
                    else{
                        Name = Name[5..];
                    }

                    tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";FUNC;TYPE=" + type + ";NAME=" + Name + ";ARG=" + newcode[i][ValStart..ValEnd].Trim() + ";");
                    tokens[j] = RemoveComments(tokens[j]);
                }

                else{
                    // Assume the code is a function or variable
                    if (newcode[i].Length > newcode[i].Replace(" = ", "").Length){
                        // Assume the line is a variable
                        j++;
                        // Get the data for the value and name
                        int Space = newcode[i].IndexOf(' ');
                        string Rest = newcode[i][Space..].Trim();
                        int SecondSpace = Rest.IndexOf(' ') + newcode[i].Length - Rest.Length;
                        string Name = newcode[i][..Space].Trim();
                        string Value = newcode[i][SecondSpace..].Trim();

                        tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";VAR;NAME=" + Name + ";VAL=" + Value);
                        tokens[j] = RemoveComments(tokens[j]);
                    }
                    else if (newcode[i].Length > newcode[i].Replace("+=", "").Length){
                        // Assume the line is a variable
                        j++;
                        // Get the data for the value and name
                        int Space = newcode[i].IndexOf(' ');
                        string Rest = newcode[i][Space..].Trim();
                        int SecondSpace = Rest.IndexOf("+= ") + newcode[i].Length - Rest.Length + 2;
                        string Name = newcode[i][..Space].Trim();
                        string Value = newcode[i][SecondSpace..].Trim();

                        tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";VARINC;NAME=" + Name + ";VAL=" + Value);
                        tokens[j] = RemoveComments(tokens[j]);
                    }
                    else if (newcode[i].Length > newcode[i].Replace("-=", "").Length){
                        // Assume the line is a variable
                        j++;
                        // Get the data for the value and name
                        int Space = newcode[i].IndexOf(' ');
                        string Rest = newcode[i][Space..].Trim();
                        int SecondSpace = Rest.IndexOf("-= ") + newcode[i].Length - Rest.Length + 2;
                        string Name = newcode[i][..Space].Trim();
                        string Value = newcode[i][SecondSpace..].Trim();

                        tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";VARDEC;NAME=" + Name + ";VAL=" + Value);
                        tokens[j] = RemoveComments(tokens[j]);
                    }
                    else if (newcode[i].Length > newcode[i].Replace("/=", "").Length){
                        // Assume the line is a variable
                        j++;
                        // Get the data for the value and name
                        int Space = newcode[i].IndexOf(' ');
                        string Rest = newcode[i][Space..].Trim();
                        int SecondSpace = Rest.IndexOf("/= ") + newcode[i].Length - Rest.Length + 2;
                        string Name = newcode[i][..Space].Trim();
                        string Value = newcode[i][SecondSpace..].Trim();

                        tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";VARDIV;NAME=" + Name + ";VAL=" + Value);
                        tokens[j] = RemoveComments(tokens[j]);
                    }
                    else if (newcode[i].Length > newcode[i].Replace("*=", "").Length){
                        // Assume the line is a variable
                        j++;
                        // Get the data for the value and name
                        int Space = newcode[i].IndexOf(' ');
                        string Rest = newcode[i][Space..].Trim();
                        int SecondSpace = Rest.IndexOf("*= ") + newcode[i].Length - Rest.Length + 2;
                        string Name = newcode[i][..Space].Trim();
                        string Value = newcode[i][SecondSpace..].Trim();

                        tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";VARMUL;NAME=" + Name + ";VAL=" + Value);
                        tokens[j] = RemoveComments(tokens[j]);
                    }
                    else if (newcode[i].Length > newcode[i].Replace("%=", "").Length){
                        // Assume the line is a variable
                        j++;
                        // Get the data for the value and name
                        int Space = newcode[i].IndexOf(' ');
                        string Rest = newcode[i][Space..].Trim();
                        int SecondSpace = Rest.IndexOf("%= ") + newcode[i].Length - Rest.Length + 2;
                        string Name = newcode[i][..Space].Trim();
                        string Value = newcode[i][SecondSpace..].Trim();

                        tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";VARMOD;NAME=" + Name + ";VAL=" + Value);
                        tokens[j] = RemoveComments(tokens[j]);
                    }
                    else if (newcode[i].Length > newcode[i].Replace("++", "").Length){
                        tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";INC;NAME=" + newcode[i][..(newcode[i].Length - 4)] + ";");
                        tokens[j] = RemoveComments(tokens[j]);
                    }
                    else if (newcode[i].Length > newcode[i].Replace("--", "").Length){
                        tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";DEC;NAME=" + newcode[i][..(newcode[i].Length - 4)] + ";");
                        tokens[j] = RemoveComments(tokens[j]);
                    }
                    //newcode[i]
                    // Check for functions
                    else if (newcode[i].Length > newcode[i].Replace("(", "").Length && !(newcode[i].Length > newcode[i].Replace(" = ", "").Length) && !(newcode[i].Length > newcode[i].Replace("func ", "").Length)){
                        string str = newcode[i];
                        char[] stringArray = str.ToCharArray();
                        Array.Reverse(stringArray);
                        string reversedPath = new string(stringArray);

                        int EndParenthesis = newcode[i].Length - reversedPath.IndexOf(")");

                        tokens.Add(i + 1 + ";SCOPE=" + tabChars.ToString() + ";FUNCCALL;" + "NAME=" + newcode[i][..newcode[i].IndexOf('(')] + ";VAL=" + newcode[i][(newcode[i].IndexOf('(') + 1)..(EndParenthesis - 1)] + ";");
                    }
                }
            }
        }
        return tokens;

    }
    static string Functions(string token){
        Dictionary<string, string> TypeLinks = new Dictionary<string, string>
        {
            {"STR", "str"},
            {"INT", "int"},
            {"BOOL", "bool"},
            {"LIST", "array[$]"},
            {"VOID", "void"}
        };

        

        int IDX = token.IndexOf(';');
        
        token = token[(IDX + 1)..];
        IDX = token.IndexOf(';');
        int scope = Int32.Parse(token[6..IDX]);

        token = token[(IDX + 1)..];
        IDX = token.IndexOf(';');
        token = token[(IDX + 1)..];

        IDX = token.IndexOf(';');
        string type = token[5..IDX];

        token = token[(IDX + 1)..];
        IDX = token.IndexOf(';');
        string Name = token[5..IDX];

        token = token[(IDX + 1)..];
        IDX = token.IndexOf(';');
        string Arguments = token[5..(IDX - 1)];
        //List<string> ArgumentsLST = detectVariables(token[5..(IDX - 1)]);
//
        //List<string> Names = new List<string>();
//
        //for (int j = 0; j < ArgumentsLST.Count; j++){
        //    Names.Add(ArgumentsLST[j][(ArgumentsLST[j].IndexOf("NAME=") + 5)..ArgumentsLST[j].IndexOf(";VAL")]);
        //}
//
        //string Allocations = "\n";
        //string Freeings = "\n";
//
        //for (int j = 0; j < Names.Count; j++){
        //    Allocations += "malloc(" + Names[j] + ");\n";
        //    Freeings += "free(" + Names[j] + ");\n";
        //}
        //Allocations += "\n";
        //Freeings += "\n";
//
        //return new List<string>{String.Join("", Enumerable.Repeat("    ", scope)) + TypeLinks[type] + " " + Name + "(" + Arguments + ")" + "\n", Allocations, Freeings};
        return String.Join("", Enumerable.Repeat("    ", scope)) + TypeLinks[type] + " " + Name + "(" + Arguments + ")" + "\n";
    }

    static string Compiler(List<string> Tokens){
        // func int main()
        //     return 0;

        // int main(){
        //     return 0;
        // }

        string FunctionEnter = "";
        string FunctionExit = "";

        Tokens.Add("0;SCOPE=0;FUNCCALL;NAME=println;VAL=\"\";");

        // "$" is where the parameters should go
        Dictionary<string, string> FunctionLinks = new Dictionary<string, string>
        {
            {"print", "std::cout << $"},
            {"println", "std::cout << $ << std::endl"},
            {"tostring", "std::to_string"},
            {"input", "std::getline(cin, $)"},
            {"StrToInt", "std::stoi($)"},
            {"random", "rand"},
            {"random.start", "srand((unsigned) time(NULL))"},
            {"char", "static_cast<char>"},
            {"unicode", "static_cast<int>"},
            {"C++", "$"}
        };

        // "$" is where the size should go
        Dictionary<string, string> TypeLinks = new Dictionary<string, string>
        {
            {"STR", "std::string"},
            {"INT", "int"},
            {"BOOL", "bool"},
            {"LIST", "array[$]"}
        };

        string code = "#include <iostream>\n#include<cstdlib>\nusing namespace std;\n#include <string>\n#define str std::string\n";

        int pastscope = 0;

        for (int i = 0; i < Tokens.Count; i++){
            
            int IDX = Tokens[i].IndexOf(';');
            int linenumber = Int32.Parse(Tokens[i][..IDX]);
            Tokens[i] = Tokens[i][(IDX + 1)..];

            IDX = Tokens[i].IndexOf(';');
            int scope = Int32.Parse(Tokens[i][6..IDX]);
            Tokens[i] = Tokens[i][(IDX + 1)..];

            if (scope > pastscope){
                code += String.Join("", Enumerable.Repeat("{", scope - pastscope));
            }

            if (scope < pastscope){
                code += String.Join("", Enumerable.Repeat("}", pastscope - scope));
            }

            code += FunctionEnter;
            FunctionEnter = "";

            IDX = Tokens[i].IndexOf(';');
            string Type = Tokens[i][..IDX];
            if (Type[^1] == ';'){
                Type = Type[..^1];
            }

            if (Type == "FUNC"){
                string Generated = Functions(linenumber.ToString() + ";SCOPE=" + scope.ToString() + ";" + Tokens[i]);
                code += '\n' + Generated;
            }
            if (Type == "FUNCCALL"){
                Tokens[i] = Tokens[i][(IDX + 1)..];

                IDX = Tokens[i].IndexOf(';');
                string Name = Tokens[i][5..IDX];
                Tokens[i] = Tokens[i][(IDX + 1)..];

                IDX = Tokens[i].IndexOf(';');
                string Value = Tokens[i][4..IDX];

                if (FunctionLinks.ContainsKey(Name)){
                    code += String.Join("", Enumerable.Repeat("    ", scope)) + FunctionLinks[Name].Replace("$", Value) + ";\n";
                }
                else{
                    code += String.Join("", Enumerable.Repeat("    ", scope)) + Name + "(" + Value + ");\n";
                }
            }

            if (Type == "RETURN"){
                IDX = Tokens[i].IndexOf(';');
                string Value = Tokens[i][(IDX + 1)..];

                code += FunctionExit + String.Join("", Enumerable.Repeat("    ", scope)) + "return " + Value + "\n";
            }
            
            if (Type == "STR"){
                IDX = Tokens[i].IndexOf(';');

                string Name = Tokens[i][(IDX + 6)..];

                Tokens[i] = Tokens[i][(IDX + 1)..];
                IDX = Name.IndexOf(";");
                Name = Name[..IDX];

                IDX = Tokens[i].IndexOf(";");
                string Value = Tokens[i][(IDX + 5)..^1];

                code += String.Join("", Enumerable.Repeat("    ", scope)) + "std::string " + Name + " = " + Value + ";\n";
            }

            if (Type == "INT"){
                IDX = Tokens[i].IndexOf(';');

                string Name = Tokens[i][(IDX + 6)..];

                Tokens[i] = Tokens[i][(IDX + 1)..];
                IDX = Name.IndexOf(";");
                Name = Name[..IDX];

                IDX = Tokens[i].IndexOf(";");
                string Value = Tokens[i][(IDX + 5)..^1];

                code += String.Join("", Enumerable.Repeat("    ", scope)) + "int " + Name + " = " + Value + ";\n";
            }

            if (Type == "BOOL"){
                IDX = Tokens[i].IndexOf(';');

                string Name = Tokens[i][(IDX + 6)..];

                Tokens[i] = Tokens[i][(IDX + 1)..];
                IDX = Name.IndexOf(";");
                Name = Name[..IDX];

                IDX = Tokens[i].IndexOf(";");
                string Value = Tokens[i][(IDX + 5)..^1];

                code += String.Join("", Enumerable.Repeat("    ", scope)) + "bool " + Name + " = " + Value + ";\n";
            }

            if (Type == "IF"){
                string Conditional = Tokens[i][(Tokens[i].IndexOf("COND=") + 5)..^1];
                code += String.Join("", Enumerable.Repeat("    ", scope)) + "if " + Conditional + "\n";
            }

            if (Type == "LIST"){
                string Name = Tokens[i];
                Name = Name[(Tokens[i].IndexOf("NAME=") + 5)..Tokens[i].IndexOf(";VAL")];

                string Value = Tokens[i][(Tokens[i].IndexOf("VAL=") + 4)..Tokens[i].IndexOf(";SIZE")];

                string Size = Tokens[i][(Tokens[i].IndexOf("SIZE=") + 5)..^1];

                string ArrayType = Tokens[i][(Tokens[i].IndexOf("TYPE=") + 5)..Tokens[i].IndexOf(";NAME")];

                //string Size = 

                code += String.Join("", Enumerable.Repeat("    ", scope)) + "array ".Replace("array", ArrayType.ToLower()) + Name + "[$] = ".Replace("$", Size) + Value + ";\n";
            }

            if (Type == "VAR"){
                IDX = Tokens[i].IndexOf(';');

                string Name = Tokens[i][(IDX + 6)..];

                Tokens[i] = Tokens[i][(IDX + 1)..];
                IDX = Name.IndexOf(";");
                Name = Name[..IDX];

                IDX = Tokens[i].IndexOf(";");
                string Value = Tokens[i][(IDX + 5)..^1];

                code += String.Join("", Enumerable.Repeat("    ", scope)) + Name + " = " + Value + ";\n";
            }

            pastscope = scope;
        }

        //if (pastscope != 0){
        //    code += String.Join("", Enumerable.Repeat("}", pastscope - 1));
        //}

        List<string> keys = new List<string>(FunctionLinks.Keys);

        for (int i = 0; i < keys.Count; i++){
            code = code.Replace(keys[i], FunctionLinks[keys[i]]);
        }

        return code[..^30];
    }
}
