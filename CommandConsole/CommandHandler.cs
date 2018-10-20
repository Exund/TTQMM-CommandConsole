using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Exund.CommandConsole
{
    class CommandHandler : MonoBehaviour
    {
        private int ID = 391476;
        private GUIContent output = new GUIContent("");
        private Vector2 scrollPos = Vector2.zero;
        private bool visible = false;

        public static Dictionary<string, TTCommand> Commands = new Dictionary<string, TTCommand>();
        public static string info = "<color=yellow>{0}</color>";
        public static string error = "<color=red>{0}</color>";

        private string expr = "";

        private List<string> history = new List<string>();
        private int historyIndex = 0;
        private string last = "";

        private void Start()
        {
            output.text = string.Format(info, "Type \"Help\" to get a list of available commands\nType \"Clear\" to clear the console");
        }

        private void OnGUI()
        {
            if (!visible) return;
            if (!CommandConsoleMod.Nuterra && CommandConsoleMod.ModExists("Nuterra.UI") )
            {
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.FullName.StartsWith("Nuterra.UI"))
                    {
                        var type = assembly.GetTypes().First(t => t.Name.Contains("NuterraGUI"));
                        CommandConsoleMod.Nuterra = (GUISkin)type.GetProperty("Skin").GetValue(null, null);
                        break;
                    }
                }
            }
            if (CommandConsoleMod.Nuterra)
            {
                GUI.skin = CommandConsoleMod.Nuterra;
            }
                
            GUI.Window(ID, new Rect(Screen.width - 500f, Screen.height - 500f, 500f, 500f), new GUI.WindowFunction(DoWindow), "Console");
        }

        

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                visible = !visible;
            }
        }

        private void DoWindow(int id)
        {
            bool exec = false;
            Event current = Event.current;
            if (current.isKey && current.type == EventType.KeyDown)
            {
                if (current.keyCode == KeyCode.Return && expr != "")
                {
                    exec = true; // If return is pressed
                }
                else if (current.keyCode == KeyCode.UpArrow) // If up is pressed
                {
                    if (historyIndex > 0)
                    {
                        if (historyIndex == history.Count)
                        {
                            last = expr; // Save current command
                        }
                        historyIndex--;
                        expr = history[historyIndex];
                    }
                }
                else if (current.keyCode == KeyCode.DownArrow)
                {
                    if (historyIndex < history.Count)
                    {
                        if (expr == history[historyIndex]) // If in correct position
                        {
                            historyIndex++;
                        }
                        if (historyIndex == history.Count)
                        {
                            expr = last; // Reload last command
                        }
                        else
                        {
                            expr = history[historyIndex]; // Load command at postiton
                        }
                    }
                }
            }

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.Label(output);
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            expr = GUILayout.TextField(expr);
            GUILayout.EndHorizontal();
            if(GUILayout.Button("Execute") || exec)
            {
                expr = expr.Trim();
                if (expr == "") return;
                output.text += "\n" + expr;
                Handler(expr);

                if (history.Count == 0 || expr != history.Last())
                {
                    historyIndex++;
                    history.Add(expr);
                    expr = "";
                }
            }
        }

        private void Handler(string input)
        {
            if (input.Split(' ').Length == 0) return;
            string commandName = input.Split(' ')[0];
            
            if (commandName == "Help")
            {
                int page = 0;
                int i = 0;

                if (input.Split(' ').Length > 1 && !int.TryParse(input.Split(' ')[1], out page))
                {
                    var cmd = input.Split(' ')[1];
                    if (!Commands.ContainsKey(cmd))
                    {
                        output.text += "\n" + string.Format(info, "The command \"" + cmd + "\" doesn't exists\nType \"Help\" to get a list of available commands");
                        return;
                    }
                    TTCommand commandHelp = Commands[cmd];
                    output.text += "\n" + string.Format(info, cmd + " : " + commandHelp.Description);
                    if (commandHelp.ArgumentsDescriptions.Keys.Count != 0)
                    {
                        foreach (string argName in commandHelp.ArgumentsDescriptions.Keys)
                        {
                            Console.WriteLine(argName.ToString());
                            try
                            {
                                //Console.WriteLine(commandHelp.ArgumentsDescriptions[argName]);

                                output.text += "\n" + argName + " : " + commandHelp.ArgumentsDescriptions[argName];
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                            }
                        }
                    }
                    //Console.WriteLine(output.text);
                }
                else
                {
                    if (page < 0) page = 0;
                    if (page > 0) page -= 1;
                    double pageCount = Math.Ceiling((double)Commands.Count / 5);
                    if (page + 1 > pageCount) page = 0;
                    output.text += "\n" + string.Format(info, "Help - Page " + (page + 1) + "/" + pageCount);
                    output.text += "\n" + string.Format(info, "Command usage : CommandName Arg1:Value1 Arg2:Value2 ArgN:ValueN\nFor more informations about a command do \"Help CommandName\"");
                    foreach (string commName in Commands.Keys)
                    {
                        if (page * 5 <= i && i < page * 5 + 5)
                        {
                            TTCommand commandHelp = Commands[commName];
                            output.text += "\n" + commName + " : " + commandHelp.Description;
                        }
                        i++;
                    }
                }
            }
            else if (commandName == "Clear")
            {
                output.text = "";
                last = "";
                history.Clear();
                historyIndex = 0;
            }
            else
            {
                if (!Commands.ContainsKey(commandName))
                {
                    output.text += "\n" + string.Format(info,"The command \"" + commandName + "\" doesn't exists\nType \"Help\" to get a list of available commands");
                }
                else
                {
                    Dictionary<string, string> args = new Dictionary<string, string>();
                    try
                    {
                        if (input.Split(' ').Length > 1)
                        {
                            for (var i = 1; i < input.Split(' ').Length; i++)
                            {
                                string[] arg = input.Split(' ')[i].Split(':');
                                args.Add(arg[0], arg[1]);
                            }
                        }
                    }
                    catch
                    {
                        output.text += string.Format(error, "\nBad syntax. Make sure to use \"name:value\"");
                    }

                    TTCommand command = Commands[commandName];
                    try
                    {
                        string commandOut = command.Call(args);
                        if (commandOut != null) output.text += "\n" + commandOut;
                    }
                    catch (Exception ex)
                    {
                        output.text += "\n" + string.Format(error,"An error occured in the command " + commandName);
                        Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }
        }
        
    }
}
