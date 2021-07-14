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
        private StringBuilder output = new StringBuilder();
        private Vector2 scrollPos = Vector2.zero;
        private Rect win = new Rect(Screen.width - 500f, Screen.height - 500f, 500f, 500f);
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
            output.AppendFormat(info, "Type \"Help\" to get a list of available commands\nType \"Clear\" to clear the console");

            new TTCommand("Help", "Shows help",
                delegate (Dictionary<string, string> arguments)
                {
                    StringBuilder str = new StringBuilder();
                    if (arguments.TryGetValue("Command", out string cmd))
                    {
                        if (!Commands.ContainsKey(cmd))
                        {
                            str.AppendFormat(info, $"The command \"{cmd}\" doesn't exists\nType \"Help\" to get a list of available commands");
                            return str.ToString();
                        }

                        TTCommand commandHelp = Commands[cmd];
                        str.AppendFormat(info, cmd + ": " + commandHelp.Description).AppendLine();
                        if (commandHelp.ArgumentsDescriptions.Keys.Count != 0)
                        {
                            foreach (string argName in commandHelp.ArgumentsDescriptions.Keys)
                            {
                                try
                                {
                                    str.AppendLine(argName + ": " + commandHelp.ArgumentsDescriptions[argName]);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }
                        }
                    }
                    else
                    {
                        int page = 1;
                        if (arguments.TryGetValue("Page", out string sPage))
                        {
                            int.TryParse(sPage, out page);
                        }

                        var pageCount = (int)Math.Ceiling((double)Commands.Count / 5);
                        if (page < 0) page = 0;
                        if (page >= pageCount) page = pageCount;

                        str.AppendFormat(info, $"Help - Page {page}/{pageCount}").AppendLine();
                        str.AppendFormat(info, "Command usage: CommandName Arg1Name:Value1 Arg2Name:Value2 ArgNName:ValueN (ex: TimeSet Moment:Day)\nFor more informations about a command do \"Help Command:CommandName\" (ex: Help Command:TimeSet)").AppendLine();
                        page -= 1;

                        int i = 0;
                        foreach (string commName in Commands.Keys)
                        {
                            if (page * 5 <= i && i < page * 5 + 5)
                            {
                                TTCommand commandHelp = Commands[commName];
                                str.AppendLine(commName + ": " + commandHelp.Description);
                            }
                            ++i;
                        }
                    }
                    return str.ToString();
                },
                new Dictionary<string, string> {
                    {
                        "(optional) Command",
                        "Command name"
                    },
                    {
                        "(optional) Page",
                        "Help Page (default: 1)"
                    }
                }
            );

            new TTCommand("Clear", "Clear the console output",
                delegate (Dictionary<string, string> arguments)
                {
                    output.Clear();
                    last = "";
                    history.Clear();
                    historyIndex = 0;
                    return "";
                }
            );

            Commands = Commands.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value);
        }

        private void Update()
        {
            if (Input.GetKeyDown(CommandConsoleMod.commandConsoleKeycode))
            {
                visible = !visible;
                useGUILayout = visible;
            }
        }

        private void OnGUI()
        {
            if (!visible) return;
            GUI.Window(ID, win, DoWindow, "Console");
        }

        private void DoWindow(int id)
        {
            bool exec = false;
            Event current = Event.current;
            try
            {
                if (current.isKey && current.type == EventType.KeyDown)
                {
                    if (current.keyCode == KeyCode.Return && !string.IsNullOrEmpty(expr))
                    {
                        exec = true; // If return is pressed
                        last = "";
                    }
                    else if (current.keyCode == KeyCode.UpArrow) // If up is pressed
                    {
                        if (historyIndex > 0)
                        {
                            if (historyIndex == history.Count)
                            {
                                last = expr; // Save current command
                            }
                            --historyIndex;
                            expr = history[historyIndex];
                        }
                    }
                    else if (current.keyCode == KeyCode.DownArrow)
                    {
                        if (historyIndex < history.Count)
                        {
                            if (expr == history[historyIndex]) // If in correct position
                            {
                                ++historyIndex;
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.Label(output.ToString());
            GUILayout.EndScrollView();
            //GUILayout.BeginHorizontal();
            expr = GUILayout.TextField(expr);
            //GUILayout.EndHorizontal();
            if (GUILayout.Button("Execute") || exec)
            {
                expr = expr.Trim();
                if (string.IsNullOrEmpty(expr)) return;
                output.AppendLine().AppendLine(expr);
                Handler(expr);

                if (history.Count == 0 || expr != history.Last())
                {
                    //history.RemoveRange(historyIndex, history.Count - historyIndex);
                    history.Add(expr);
                    historyIndex = history.Count;
                    expr = "";
                }
            }
        }

        private void Handler(string input)
        {
            var words = input.Split(' ');
            if (words.Length == 0) return;
            string commandName = words[0];

            /*if (commandName == "Help")
            {
                int page = 0;
                int i = 0;

                if (words.Length > 1 && !int.TryParse(words[1], out page))
                {
                    var cmd = words[1];
                    if (!Commands.ContainsKey(cmd))
                    {
                        output.text += "\n" + string.Format(info, "The command \"" + cmd + "\" doesn't exists\nType \"Help\" to get a list of available commands");
                        return;
                    }
                    TTCommand commandHelp = Commands[cmd];
                    output.text += "\n" + string.Format(info, cmd + ": " + commandHelp.Description);
                    if (commandHelp.ArgumentsDescriptions.Keys.Count != 0)
                    {
                        foreach (string argName in commandHelp.ArgumentsDescriptions.Keys)
                        {
                            //Console.WriteLine(argName.ToString());
                            try
                            {
                                //Console.WriteLine(commandHelp.ArgumentsDescriptions[argName]);

                                output.text += "\n" + argName + ": " + commandHelp.ArgumentsDescriptions[argName];
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }
                    //Console.WriteLine(output.text);
                }
                else
                {
                    if (page < 0) page = 0;
                    if (page > 0) page -= 1;
                    var pageCount = (int)Math.Ceiling((double)Commands.Count / 5);
                    if (page >= pageCount) page = pageCount - 1;
                    output.text += "\n" + string.Format(info, "Help - Page " + (page + 1) + "/" + pageCount);
                    output.text += "\n" + string.Format(info, "Command usage: CommandName Arg1Name:Value1 Arg2Name:Value2 ArgNName:ValueN (ex: TimeSet Moment:Day)\nFor more informations about a command do \"Help CommandName\"");
                    foreach (string commName in Commands.Keys)
                    {
                        if (page * 5 <= i && i < page * 5 + 5)
                        {
                            TTCommand commandHelp = Commands[commName];
                            output.text += "\n" + commName + ": " + commandHelp.Description;
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
            {*/
            if (!Commands.ContainsKey(commandName))
            {
                output.AppendLine().AppendFormat(info, $"The command \"{commandName}\" doesn't exists\nType \"Help\" to get a list of available commands\n");
            }
            else
            {
                Dictionary<string, string> args = new Dictionary<string, string>();
                try
                {
                    if (words.Length > 1)
                    {
                        for (var i = 1; i < words.Length; i++)
                        {
                            string[] arg = words[i].Split(':');
                            args.Add(arg[0], arg[1]);
                        }
                    }

                    TTCommand command = Commands[commandName];
                    try
                    {
                        string commandOut = command.Call(args);
                        if (!string.IsNullOrEmpty(commandOut)) output.AppendLine(commandOut);//.Append(commandOut);
                    }
                    catch (Exception ex)
                    {
                        output.AppendLine().AppendFormat(error, "An error occured in the command " + commandName);
                        Console.WriteLine(ex.ToString());
                    }
                }
                catch
                {
                    output.AppendLine().AppendFormat(error, "Bad syntax. Make sure to format your arguments correctly (\"ArgumentName:Value\")");
                }
            }
            //}
        }
    }
}
