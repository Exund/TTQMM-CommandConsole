using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using ModHelper.Config;
using Nuterra.NativeOptions;

namespace Exund.CommandConsole
{
    public class CommandConsoleMod
    {
        private static GameObject _holder;

        private static FieldInfo m_Sky;

        internal static KeyCode commandConsoleKeycode = KeyCode.KeypadEnter;

        public static void Load()
        {
            ModConfig config = new ModConfig();
            int v = (int)KeyCode.KeypadEnter;
            config.TryGetConfig<int>("commandConsoleKeycode", ref v);
            commandConsoleKeycode = (KeyCode)v;

            OptionKey commandConsoleKey = new OptionKey("Command Console toggle", "Command Console", commandConsoleKeycode);
            commandConsoleKey.onValueSaved.AddListener(() =>
            {
                commandConsoleKeycode = commandConsoleKey.SavedValue;
                config["commandConsoleKeycode"] = (int)commandConsoleKeycode;
                config.WriteConfigJsonFile();
            });

            _holder = new GameObject();
            _holder.AddComponent<CommandHandler>();
            UnityEngine.Object.DontDestroyOnLoad(_holder);

            new TTCommand("Teleport", "Teleports the player to the specified X Y Z values",
                delegate(Dictionary<string, string> arguments)
                {
                    Vector3 pos = Singleton.playerTank.trans.position;

                    if (arguments.TryGetValue("X", out string argX) && float.TryParse(argX, out float floatX)) pos.x = floatX; //Quaternion.Euler(floatX, 0, 0).x;
                    if (arguments.TryGetValue("Y", out string argY) && float.TryParse(argY, out float floatY)) pos.y = floatY; //Quaternion.Euler(0, floatY, 0).y;
                    if (arguments.TryGetValue("Z", out string argZ) && float.TryParse(argZ, out float floatZ)) pos.z = floatZ; //Quaternion.Euler(0, 0, floatZ).z;

                    Vector3 b = Singleton.cameraTrans.position - Singleton.playerTank.boundsCentreWorld;
                    Vector3 vector2 = Singleton.cameraTrans.rotation * pos;
                    vector2 = vector2.SetY(0f).normalized * 100f;
                    Vector3 vector3 = Singleton.playerTank.boundsCentreWorld + vector2;
                    Singleton.playerTank.visible.Teleport(pos, Singleton.playerTank.trans.rotation, false);
                    vector3 = Singleton.Manager<ManWorld>.inst.ProjectToGround(vector3, true) + Vector3.up;
                    Singleton.Manager<CameraManager>.inst.ResetCamera(pos + b, Singleton.cameraTrans.rotation);

                    return "Player teleported to " + pos.ToString();
                },
                new Dictionary<string, string>
                {
                    {
                        "X",
                        "X position"
                    },
                    {
                        "Y",
                        "Y position"
                    },
                    {
                        "Z",
                        "Z position"
                    }
                }
            );

            new TTCommand("SetGravity", "Set the game Gravity",
                delegate(Dictionary<string, string> arguments)
                {
                    Vector3 newGrav = Physics.gravity;

                    if (arguments.TryGetValue("X", out string argX) && float.TryParse(argX, out float floatX)) newGrav.x = floatX;
                    if (arguments.TryGetValue("Y", out string argY) && float.TryParse(argY, out float floatY)) newGrav.y = floatY;
                    if (arguments.TryGetValue("Z", out string argZ) && float.TryParse(argZ, out float floatZ)) newGrav.z = floatZ;
                    Physics.gravity = newGrav;

                    return "Gravity set to to " + Physics.gravity.ToString();
                },
                new Dictionary<string, string>
                {
                    {
                        "(optional) X",
                        "Gravity on X axis (default: 0)"
                    },
                    {
                        "(optional) Y",
                        "Gravity on Y axis (default: -30)"
                    },
                    {
                        "(optional) Z",
                        "Gravity on Z axis (default: 0)"
                    }
                }
            );

            new TTCommand("SetVelocity", "Set the velocity of the player tech",
                delegate(Dictionary<string, string> arguments)
                {
                    if (!Singleton.playerTank) return string.Format(CommandHandler.info, "Specified Tech not found");

                    Vector3 newVel = Singleton.playerTank.rbody.velocity;

                    if (arguments.TryGetValue("X", out string argX) && int.TryParse(argX, out int intX)) newVel.x = intX;
                    if (arguments.TryGetValue("Y", out string argY) && int.TryParse(argY, out int intY)) newVel.y = intY;
                    if (arguments.TryGetValue("Z", out string argZ) && int.TryParse(argZ, out int intZ)) newVel.z = intZ;
                    Singleton.playerTank.rbody.velocity = newVel;

                    return "Tech velocity set to " + newVel.ToString();
                },
                new Dictionary<string, string>
                {
                    {
                        "(optional) X",
                        "Velocity on the X axis"
                    },
                    {
                        "(optional) Y",
                        "Velocity on the Y axis"
                    },
                    {
                        "(optional) Z",
                        "Velocity on the Z axis"
                    }
                }
            );

            new TTCommand("SetRotation", "Set the rotation of the player tech",
                delegate(Dictionary<string, string> arguments)
                {
                    if (!Singleton.playerTank) return string.Format(CommandHandler.info, "Specified Tech not found");

                    Vector3 newRot = Singleton.playerTank.trans.rotation.eulerAngles;

                    if (arguments.TryGetValue("X", out string argX) && int.TryParse(argX, out int intX)) newRot.x = intX; //Quaternion.Euler(intX, 0, 0).x;
                    if (arguments.TryGetValue("Y", out string argY) && int.TryParse(argY, out int intY)) newRot.y = intY; //Quaternion.Euler(0, intY, 0).y;
                    if (arguments.TryGetValue("Z", out string argZ) && int.TryParse(argZ, out int intZ)) newRot.z = intZ; //Quaternion.Euler(0, 0, intZ).z;
                    Singleton.playerTank.trans.rotation = Quaternion.Euler(newRot);

                    return "Tech rotated to " + newRot.ToString();
                },
                new Dictionary<string, string>
                {
                    {
                        "(optional) X",
                        "Rotation on X axis (in degrees)"
                    },
                    {
                        "(optional) Y",
                        "Rotation on Y axis (in degrees)"
                    },
                    {
                        "(optional) Z",
                        "Rotation on Z axis (in degrees)"
                    }
                }
            );


            m_Sky = typeof(ManTimeOfDay).GetField("m_Sky", BindingFlags.Instance | BindingFlags.NonPublic);

            new TTCommand("TimeSet", "Set the time of day",
                delegate(Dictionary<string, string> arguments)
                {
                    if (arguments.TryGetValue("Moment", out string moment))
                    {
                        switch (moment)
                        {
                            case "Day":
                                Singleton.Manager<ManTimeOfDay>.inst.SetTimeOfDay(5, 30, 0);
                                break;

                            case "Night":
                                Singleton.Manager<ManTimeOfDay>.inst.SetTimeOfDay(18, 30, 0);
                                break;

                            case "Noon":
                                Singleton.Manager<ManTimeOfDay>.inst.SetTimeOfDay(12, 0, 0);
                                break;

                            case "Midnight":
                                Singleton.Manager<ManTimeOfDay>.inst.SetTimeOfDay(0, 0, 0);
                                break;

                            default:
                                return string.Format(CommandHandler.info, "Incorrect moment");
                        }
                    }
                    else if (arguments.TryGetValue("Hour", out string sHour))
                    {
                        if (int.TryParse(sHour, out int hour)) Singleton.Manager<ManTimeOfDay>.inst.SetTimeOfDay(hour, 0, 0);
                    }
                    else
                    {
                        return string.Format(CommandHandler.info, "Missing argument Moment/Hour");
                    }

                    return "Time set to " + Singleton.Manager<ManTimeOfDay>.inst.TimeOfDay;
                },
                new Dictionary<string, string>
                {
                    {
                        "Moment",
                        "Moment of the day ( Day | Night | Noon | Midnight )"
                    },
                    {
                        "Hour",
                        "Hour of the day"
                    }
                }
            );

            new TTCommand("SetDayLength", "Set day length",
                delegate(Dictionary<string, string> arguments)
                {
                    if (arguments.TryGetValue("Length", out string sLength))
                    {
                        if (float.TryParse(sLength, out float length)) ((TOD_Sky)m_Sky.GetValue(Singleton.Manager<ManTimeOfDay>.inst)).Components.Time.DayLengthInMinutes = length;
                    }
                    else
                    {
                        return string.Format(CommandHandler.info, "Missing argument Length");
                    }

                    return $"Day length set to {sLength} minutes";
                },
                new Dictionary<string, string>
                {
                    {
                        "Length",
                        "Day length in minutes"
                    }
                }
            );

            new TTCommand("TimeToggle", "Toggle the Day/Night Cycle",
                delegate(Dictionary<string, string> arguments)
                {
                    Singleton.Manager<ManTimeOfDay>.inst.TogglePause();
                    return "Time progression set to " + ((TOD_Sky)m_Sky.GetValue(Singleton.Manager<ManTimeOfDay>.inst)).Components.Time.ProgressTime;
                }
            );

            new TTCommand("SetTimeScale", "Set the simulation speed multiplier",
                delegate(Dictionary<string, string> arguments)
                {
                    if (arguments.TryGetValue("Scale", out string sScale))
                    {
                        if (float.TryParse(sScale, out float scale)) Time.timeScale = scale;
                    }
                    else
                    {
                        return string.Format(CommandHandler.info, "Missing argument Scale");
                    }

                    return "Simulation time scale set to " + sScale;
                }, new Dictionary<string, string>
                {
                    {
                        "Scale",
                        "Simulation speed multiplier (default: 1)"
                    }
                }
            );
        }
    }
}
