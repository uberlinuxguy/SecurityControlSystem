using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.


        // Secuirty Control System \\\
        // Version 0.1 Beta.
        // Based on the Lock Script by SioxerNikkita
        // Credit Link: https://steamcommunity.com/sharedfiles/filedetails/?id=850692131


        // To be used with a keypad like this one by SioxerNikkita
        // Url: https://steamcommunity.com/sharedfiles/filedetails/?id=850671150

        /*
         *
         *  Information - This script is meant to be used as a central security
         *  system for a bunch of doors and keypads.  
         *  
         *  ***** THIS SCRIPT IS STILL IN BETA!!!
         *  
         *  Report issues with this script on the github page
         *    URL: https://github.com/uberlinuxguy/SecurityController
         *    
         *    Instructions: 
         *      1) Add a door.
         *      2) Add a keypad near the door
         *      3) Add a keypad LCD to the keypad (See the link for the keypad)
         *      4) Program the keypad buttions: 
         *          a) Number buttons should be programmed like with run args like this
         *              i) 1 KeypadName
         *          b) The button should trigger a run on the security controller PB and pass 
         *             the args of the button pressed, a single space, and the name of the keypad
         *      5) Add the custom data to the keypad
         *          a) custom data should follow the following format:
         *                door_name=doorName
         *                lcd_name=KeypadLCDName
         *                passcode=1234:SUCCESS!
         *                passcode=4321:Go In!
         *          b) The door_name is assigned the name of the door entrered in the terminal, it
         *             is currently recommended not to use spaces and not to have 2 doors with the same name
         *          c) The lcd_name is  assigned the name of the lcd entrered in the terminal.  The 
         *             same recommendations apply for formatting
         *          d) Any number of passcodes can be specified with matching messages.  If codes are the
         *             same, the first one found will be used for the message.
         *      
         *      6) add the following string to the keypad name "[keypad]"  The control program will pick up
         *         and register the new pad.
         *         
         *  Known Issues: 
         *      - once a keypad is registered, it will not be updated.  You would have to recompile the program
         *        to restart and register ALL keypads.
         *      - The upper limit of keypads is currently not known.  This will hopefully be tested.
         *      - The LCD doesn't change state after pressing OK.  you have to manually clear it by pressing
         *        the clear button or starting to enter a new password.
         *        
         *        
         *        ***** Comments below this line are not yet guaranteed to be up to date.  
         *        *** I plan on better documentation as I use and refine the script out of beta.
         *        *** PR's are always welcome!

        */

        const int MaxTries = 3; // Maximum number of tries before the keypad locks.

        public class LCDHandler
        {
            public IMyTextPanel LCD;
            private IMyGridTerminalSystem gts;

            private float PasswordFontSize = 6; // This is the font size of the hidden characters for password
            private char PasswordCharacter = '*'; //This is the character that substitutes the digits.
            private Color PasswordFontColor = new Color(255, 255, 255); // Font Color of the Password Screen
            private Color PasswordBackColor = new Color(0, 0, 150); // Background Color of the Password Screen

            private float ReadyFontSize = 4; // This is the font size of the Ready Message
            private string ReadyMessage = "    Ready"; // This is the message displayed when the Keypad is ready.
            private Color ReadyFontColor = new Color(255, 255, 255); // Font Color of the Ready Message
            private Color ReadyBackColor = new Color(0, 0, 150); // Background Color of the Ready Message

            private float SuccessFontSize = 2; // This is the font size of the Success Message
            private Color SuccessFontColor = new Color(0, 0, 0); // Font Color of Success Messages
            private Color SuccessBackColor = new Color(0, 150, 0); // Background Color of Success Messages

            private float FailureFontSize = 4; // This is the font size of the Failure Message
            private string FailureMessage = "   Failed"; //This is the message displayed when the user typed wrong.
            private Color FailureFontColor = new Color(255, 255, 255); // Font Color of the Failure Message
            private Color FailureBackColor = new Color(150, 0, 0); // Background Color of the Failure Message

            private float LockedFontSize = 5; // This is the font size of the Locked Message
            private string LockedMessage = " LOCKED!"; //This is the message displayed when the keypad is locked.
            private Color LockedFontColor = new Color(255, 255, 255); // Font Color of the Locked Message
            private Color LockedBackColor = new Color(150, 0, 0); // Background color of the Locked Message

            public LCDHandler(string LCDName, IMyGridTerminalSystem gts, Program myProgram)
            {
                this.gts = gts;
                if (LCDName != null && LCDName.ToLower() != "null")
                {
                    // get reference to the LCD
                    myProgram.Echo("Looking for LCD: " + LCDName);
                    LCD = (IMyTextPanel)gts.GetBlockWithName(LCDName);
                    if (LCD != null)
                    {
                        myProgram.Echo("Found");
                        LCD.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                    }
                    else
                    {
                        myProgram.Echo("Not Found");
                    }

                }
            }

            public void Password(string argument)
            {
                if (LCD != null)
                {
                    LCD.SetValueFloat("FontSize", PasswordFontSize);
                    LCD.SetValueColor("FontColor", PasswordFontColor);
                    LCD.SetValueColor("BackgroundColor", PasswordBackColor);
                    LCD.WriteText(new String(PasswordCharacter, argument.Length), false);
                }
            }


            public void Ready()
            {
                if (LCD != null)
                {
                    LCD.SetValueFloat("FontSize", ReadyFontSize);
                    LCD.SetValueColor("FontColor", ReadyFontColor);
                    LCD.SetValueColor("BackgroundColor", ReadyBackColor);
                    LCD.WriteText(ReadyMessage, false);
                }
            }
            public void Success(string Name)
            {
                if (LCD != null)
                {
                    LCD.SetValueFloat("FontSize", SuccessFontSize);
                    LCD.SetValueColor("FontColor", SuccessFontColor);
                    LCD.SetValueColor("BackgroundColor", SuccessBackColor);
                    LCD.WriteText(Name, false);
                }
            }
            public void Failure()
            {
                if (LCD != null)
                {
                    LCD.SetValueFloat("FontSize", FailureFontSize);
                    LCD.SetValueColor("FontColor", FailureFontColor);
                    LCD.SetValueColor("BackgroundColor", FailureBackColor);
                    LCD.WriteText(FailureMessage, false);
                }
            }
            public void Locked()
            {
                if (LCD != null)
                {
                    LCD.SetValueFloat("FontSize", LockedFontSize);
                    LCD.SetValueColor("FontColor", LockedFontColor);
                    LCD.SetValueColor("BackgroundColor", LockedBackColor);
                    LCD.WriteText(LockedMessage, false);
                }
            }
        }

        Dictionary<string, Keypad> keypads = new Dictionary<string, Keypad>();
        //bool Locked = false;

        public Program()
        {
            // One time setup.

            // Scan for and add Keypads
            ScanForKeypads(GridTerminalSystem);
            Runtime.UpdateFrequency = UpdateFrequency.Update10; // update every 10 ticks.

        }

        public class DoorHandler
        {
            public string name;

            public IMyDoor doorObj;

            private StateMachine doorStateMachine;

            public DoorHandler(string doorName, IMyGridTerminalSystem gts)
            {
                name = doorName;

                // TODO: find the door.
                doorObj = (IMyDoor)gts.GetBlockWithName(name);


                // initialize the statemachine
                doorStateMachine = new StateMachine();
                doorStateMachine.overrideState("closing");

                // Set up the transititions, default to locked if no door.
                doorStateMachine.transitions.Add("enable", () => {
                    if (doorObj == null)
                        return "locked";
                    return (doorObj.Enabled) ? "opening" : "enable";
                });
                doorStateMachine.transitions.Add("opening", () => {
                    if (doorObj == null)
                        return "locked";
                    return (doorObj.Status == DoorStatus.Open) ? "open_wait" : "opening";
                });
                doorStateMachine.transitions.Add("timer_wait", () => {
                    if (doorObj == null)
                        return "locked";
                    return (doorStateMachine.timerActive()) ? "timer_wait" : "closing";
                });
                doorStateMachine.transitions.Add("closing", () => {
                    if (doorObj == null)
                        return "locked";
                    return (doorObj.Status == DoorStatus.Closed) ? "locked" : "closing";
                });

                // no transition for locked.

                doorStateMachine.actions.Add("enable", () => {
                    if (doorObj == null)
                        return;
                    doorObj.Enabled = true;
                });
                doorStateMachine.actions.Add("opening", () => {
                    if (doorObj == null)
                        return;
                    if (doorObj.Status == DoorStatus.Opening) { return; }
                    doorObj.OpenDoor();
                });
                doorStateMachine.actions.Add("open_wait", () => {
                    if (doorObj == null)
                        return;
                    doorStateMachine.startTimer(5);
                    doorStateMachine.overrideState("timer_wait");
                });
                doorStateMachine.actions.Add("closing", () => {
                    if (doorObj == null)
                        return;
                    if (doorObj.Status == DoorStatus.Closing) { return; }
                    doorObj.Enabled = true;
                    doorObj.CloseDoor();
                });
                doorStateMachine.actions.Add("locked", () => {
                    if (doorObj == null)
                        return;
                    doorObj.Enabled = false;
                });


            }

            public void openDoor()
            {
                doorStateMachine.overrideState("enable");
            }

            public string getState()
            {
                return doorStateMachine.getState();
            }

            public void Update()
            {
                // update for the door, called from the keypad which is called from Main() with no args.
                doorStateMachine.Update();
            }

        }

        public class Keypad
        {
            public string Name;
            private string DoorName;
            public Dictionary<string, string> passcodes;
            public LCDHandler lcdHandler;
            private int CurrentTries;
            private string password;
            private DoorHandler doorHandler;

            private bool Locked;

            private StateMachine keypadStateMachine;


            public Keypad(string keypadName)
            {
                Name = keypadName;
                Locked = false;
                passcodes = new Dictionary<string, string>();
                Reset();

                // initialize the statemachine
                keypadStateMachine = new StateMachine();
                keypadStateMachine.overrideState("idle");

                // Set up the transititions, default to idle
                keypadStateMachine.transitions.Add("timer_wait", () => {
                    return (keypadStateMachine.timerActive()) ? "timer_wait" : "idle";
                });

                keypadStateMachine.actions.Add("failed", () => {
                    password = "";
                    keypadStateMachine.startTimer(3);
                    keypadStateMachine.overrideState("timer_wait");
                });
                keypadStateMachine.actions.Add("passed", () => {
                    password = "";
                    keypadStateMachine.startTimer(3);
                    keypadStateMachine.overrideState("timer_wait");
                });
                keypadStateMachine.actions.Add("clear", () => {
                    password = "";
                    keypadStateMachine.overrideState("idle");
                });

                keypadStateMachine.actions.Add("idle", () =>
                {
                    if ((CurrentTries < MaxTries) && doorHandler.getState() == "locked")
                    {
                        if(password == "")
                            Clear();
                        // the door has closed, unlock the keypad if it's not locked from tries.
                        Locked = false;
                    }
                });

            }

            public void setDoorName(string doorName, IMyGridTerminalSystem gts)
            {
                DoorName = doorName;
                // create a doorHandler object.
                doorHandler = new DoorHandler(DoorName, gts);

            }

            public string getDoorName()
            {
                return DoorName;
            }

            public bool Check(string TestPassword)
            {
                if (password == TestPassword)
                {
                    return true;
                }
                return false;

            }

            public void AddCode(string passcode, string message)
            {
                passcodes.Add(passcode, message);
            }

            public void Clear()
            {
                if (CurrentTries < MaxTries)
                {
                    if (lcdHandler != null)
                        lcdHandler.Ready();
                    password = "";
                }
                else
                {

                    if (lcdHandler != null)
                        lcdHandler.Locked();
                    password = "";
                }
            }

            public void Update()
            {
                // will be called at the bottom of Main()
                // used for the statemachine for operating the door
                // linked to this keypad
                if (doorHandler != null)
                {
                    doorHandler.Update();
                }
                keypadStateMachine.Update();

            }

            public void Reset()
            {
                CurrentTries = 0;
                password = "";
                Clear();
                Locked = false;
            }

            public void Clicked(string btn)
            {

                if (Locked)
                {
                    if (btn == "CMD_RESET")
                    {
                        Reset();
                    }
                    if ((CurrentTries < MaxTries) && doorHandler.getState() == "locked")
                    {
                        // the door status is locked, it has cycled back to closed, so reset
                        Reset();
                    }
                }
                else
                {
                    switch (btn)
                    {
                        case "CMD_RESET":
                            Reset();
                            break;
                        case "CMD_CLEAR":
                            Clear();
                            break;
                        case "CMD_OVERRIDE":
                            doorHandler.openDoor();
                            break;
                        case "CMD_OK":
                            if (CurrentTries < MaxTries)
                            {
                                // if this password is in the passcodes for the keypad, SUCCESS!
                                if (passcodes.ContainsKey(password))
                                {
                                    // passcode is in the passcodes array
                                    if (lcdHandler != null)
                                        lcdHandler.Success(passcodes[password]);
                                    CurrentTries = 0;
                                    Locked = true;
                                    doorHandler.openDoor();
                                    keypadStateMachine.overrideState("passed");

                                }
                                else
                                {
                                    // code not found, log a failure and lock if max tries is reached
                                    CurrentTries += 1;
                                    if (CurrentTries < MaxTries)
                                    {
                                        if (lcdHandler != null)
                                            lcdHandler.Failure();
                                        keypadStateMachine.overrideState("failed");
                                    }
                                    else
                                    {
                                        if (lcdHandler != null)
                                            lcdHandler.Locked();
                                        Locked = true;
                                    }
                                }
                            }
                            break;
                        default:
                            password += btn;
                            if (lcdHandler != null)
                                lcdHandler.Password(password);
                            break;
                    }
                }
            }
        }
        public class MockTimer
        {
            DateTime myTime;

            public MockTimer(int seconds)
            {
                myTime = DateTime.Now.AddSeconds(seconds);
            }

            public bool isActive
            {
                get
                {
                    return DateTime.Now < myTime;
                }
            }
        }
        public class StateMachine
        {

            private string currentState;

            public delegate void myAction();

            public delegate string myTransition();

            public Dictionary<string, myAction> actions;

            public Dictionary<string, myTransition> transitions;

            private MockTimer timer;

            public StateMachine()
            {
                actions = new Dictionary<string, myAction>();
                transitions = new Dictionary<string, myTransition>();

            }

            public void startTimer(int seconds)
            {
                timer = new MockTimer(seconds);
            }

            public bool timerActive()
            {
                return timer.isActive;
            }

            public void overrideState(string state)
            {
                currentState = state;
            }

            public string getState()
            {
                return currentState;
            }


            public string Update()
            {
                // check the transition
                if (transitions.ContainsKey(currentState))
                {
                    currentState = (string)transitions[currentState]();
                }

                // then run the action
                if (actions.ContainsKey(currentState))
                {
                    actions[currentState]();
                }


                return currentState;

            }
        }

        public void ScanForKeypads(IMyGridTerminalSystem gts)
        {
            // Scan for blocks with the "keypad" tag (make this customizable)
            // and add them to the dictionary.

            // Read in the config data from custom data
            // custom data example: 
            //      door_name = <name of the door>
            //      pass_code = passcode:Message to display
            //      pass_code = passcod2:Second Msg

            List<IMyTerminalBlock> keypadsSearch = new List<IMyTerminalBlock>();
            gts.SearchBlocksOfName("[keypad]", keypadsSearch);
            Echo("Before scan, keypads is: " + keypads.Count.ToString());
            if (keypadsSearch.Count > 0)
            {
                for (int i = 0; i < keypadsSearch.Count; i++)
                {

                    if (!keypads.ContainsKey(keypadsSearch[i].CustomName))
                    {
                        string tmpName = keypadsSearch[i].CustomName;
                        // key was not found add the keypad
                        string CustomData = keypadsSearch[i].CustomData;
                        List<string> dataLines = CustomData.Split('\n').ToList(); ;
                        if (dataLines.Count > 0)
                        {
                            // create a temp instance of the keypad
                            Keypad tmpKeypad = new Keypad(tmpName);
                            // now split the lines
                            for (int j = 0; j < dataLines.Count; j++)
                            {
                                //var passcodeIn = "";
                                if (dataLines[j].Length < 1)
                                    continue;
                                if (dataLines[j][0] == '\n' || dataLines[j][0] == '#')
                                    continue;
                                List<string> lineElements = dataLines[j].Split('=').ToList();
                                if (lineElements.Count != 2)
                                {
                                    Echo("Error: Keypad: " + tmpName + " invalid custom data");
                                    tmpKeypad = null;
                                }
                                else
                                {
                                    string name = lineElements[0];
                                    string value = lineElements[1];
                                    if (name == "" || value == "")
                                    {
                                        Echo("Error: Keypad: " + tmpName + " invalid custom data");
                                        tmpKeypad = null;
                                    }
                                    else
                                    {
                                        // process the custom data
                                        switch (name)
                                        {
                                            case "door_name":
                                                if (tmpKeypad != null)
                                                    tmpKeypad.setDoorName(value, gts);
                                                break;
                                            case "passcode":
                                                List<string> passElements = value.Split(new[] { ':' }, 2).ToList();
                                                if (passElements.Count != 2)
                                                {
                                                    Echo("Error: Keypad: " + tmpName + " invalid custom data");
                                                    tmpKeypad = null;
                                                }
                                                else
                                                {
                                                    // element 0 is code, element 1 is message.
                                                    if (passElements[0] == "" || passElements[1] == "")
                                                    {
                                                        Echo("Error: Keypad: " + tmpName + " invalid custom data");
                                                        tmpKeypad = null;
                                                    }
                                                    if (tmpKeypad != null)
                                                    {
                                                        //Echo("Added Passcode: " + passElements[0] + " to keypad " + tmpKeypad.Name);
                                                        /*int index = passElements[1].IndexOf("\\n");
                                                        if(index > -1)
                                                        {
                                                            string tmpString = "";
                                                            tmpString = passElements[1].Substring(0, index) + " \n";
                                                            tmpString = tmpString + passElements[1].Substring(index + 3);
                                                        }*/
                                                        tmpKeypad.passcodes.Add(passElements[0], passElements[1].Replace("\\n", "\n"));
                                                    }
                                                }
                                                break;
                                            case "lcd_name":
                                                LCDHandler lcd = new LCDHandler(value, gts, this);
                                                if (lcd != null)
                                                {
                                                    if (tmpKeypad != null)
                                                    {
                                                        tmpKeypad.lcdHandler = lcd;
                                                        tmpKeypad.Reset();
                                                    }
                                                }
                                                else
                                                {
                                                    Echo("Error: Keypad: " + tmpName + " invalid LCD name");
                                                    tmpKeypad = null;
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        if (tmpKeypad == null)
                                        {
                                            break; // break the for loop
                                        }
                                    }
                                }

                            }
                            // last check before we add the keypad
                            if (tmpKeypad != null &&
                                tmpKeypad.Name != "" &&
                                tmpKeypad.getDoorName() != "" &&
                                tmpKeypad.passcodes.Count > 0)
                            {
                                keypads.Add(tmpKeypad.Name, tmpKeypad);
                            }
                            else
                            {
                                Echo("Error: Cannot add keypad.  Invalid Custom Data.");
                                if (tmpKeypad == null)
                                {
                                    Echo("  tmpKeyPad is null");
                                }
                                else
                                {
                                    Echo("  name: " + tmpKeypad.Name);
                                    Echo("  door: " + tmpKeypad.getDoorName());
                                    Echo("  pcnt: " + tmpKeypad.passcodes.Count.ToString());
                                }

                                tmpKeypad = null;
                            }

                        }
                    } // end contians if
                }
            }

            Echo("After Scan keypads is: " + keypads.Count.ToString());
        }

        public void Main(string argument)
        {
            // need the name of the 
            // calling keypad, so need a split here

            if (argument == "")
            {
                // no arguments, scan for keypads

                ScanForKeypads(GridTerminalSystem);

                //then run update of all registered keypads.
                var keypadEnum = keypads.GetEnumerator();
                while (keypadEnum.MoveNext())
                {
                    Keypad current = keypadEnum.Current.Value;
                    current.Update();
                }
            }
            else
            {
                // Argument specified, so let's find the specified
                // keypad and run the command

                // split the args on spaces... maybe.
                string[] args = argument.Split(new[] { ' ' }, 2);
                string cmd = args[0];
                string keypadName = args[1];

                // Find the keypad with this name, first search for the bock
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.SearchBlocksOfName(keypadName, blocks, null);
                Echo("Found: " + blocks.Count.ToString());
                Echo("   with name matching: " + keypadName);
                if (blocks.Count == 1)
                {
                    if (keypads.ContainsKey(blocks[0].CustomName))
                    {
                        // found a keypad.
                        keypads[blocks[0].CustomName].Clicked(cmd);
                    }
                }
                else
                {
                    Echo("Search returned multiple results. Invalid Keypad Name");
                }

            }
        }
    }
}
 