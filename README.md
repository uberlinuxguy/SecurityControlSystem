# Secuirty Control System 

Version 0.1 Beta.  
Based on the Lock Script by SioxerNikkita  
Credit Link: https://steamcommunity.com/sharedfiles/filedetails/?id=850692131  


To be used with a keypad like this one by SioxerNikkita  
Url: https://steamcommunity.com/sharedfiles/filedetails/?id=850671150  


# Information 
This script is meant to be used as a central security system for a bunch of doors and keypads.    
  
  ***** THIS SCRIPT IS STILL IN BETA!!!  
  
Report issues with this script on the github page  
  URL: https://github.com/uberlinuxguy/SecurityController  
    
# Instructions:   
      1) Add a door.  
      2) Add a keypad near the door  
      3) Add a keypad LCD to the keypad (See the link for the keypad)  
      4) Program the keypad buttions:   
          a) Number buttons should be programmed like with run args like this  
              i) 1 KeypadName 
          b) The button should trigger a run on the security controller PB and pass   
             the args of the button pressed, a single space, and the name of the keypad  
      5) Add the custom data to the keypad  
          a) custom data should follow the following format:  
                door_name=doorName  
                lcd_name=KeypadLCDName  
                passcode=1234:SUCCESS!  
                passcode=4321:Go In!  
          b) The door_name is assigned the name of the door entrered in the terminal, it  
             is currently recommended not to use spaces and not to have 2 doors with the same name  
          c) The lcd_name is  assigned the name of the lcd entrered in the terminal.  The   
             same recommendations apply for formatting  
          d) Any number of passcodes can be specified with matching messages.  If codes are the  
             same, the first one found will be used for the message.  
      
      6) add the following string to the keypad name "[keypad]"  The control program will pick up  
         and register the new pad.  
           
# Known Issues:   
      - once a keypad is registered, it will not be updated.  You would have to recompile the program  
        to restart and register ALL keypads.  
      - The upper limit of keypads is currently not known.  This will hopefully be tested.  
      - The LCD doesn't change state after pressing OK.  you have to manually clear it by pressing  
        the clear button or starting to enter a new password.  
        
