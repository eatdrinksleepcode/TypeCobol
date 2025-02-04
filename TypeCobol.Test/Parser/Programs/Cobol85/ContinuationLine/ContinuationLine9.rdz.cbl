﻿       IDENTIFICATION DIVISION.
       PROGRAM-ID. DVZZMFT0.
       data division.
       working-storage section.
       01 var1 PIC X(120) VALUE "text1-AAAAABBBBBCCCCCDDDDDEEEEEFFFFFGGG
      -    "GG-end1".
      
       01 var2 PIC X(120) VALUE "text2-AAAAABBBBBCCCCCDDDDDEEEEEFFFFFGGG
      *with a comment
      -    "GG-end2".
      
      *with a blank line
       01 var3 PIC X(120) VALUE "text3-AAAAABBBBBCCCCCDDDDDEEEEEFFFFFGGG
      
      -    "GG-end3".
      
      *with a blank continuation
       01 var4 PIC X(120) VALUE "text4-AAAAABBBBBCCCCCDDDDDEEEEEFFFFFGGG
      -
      -    "GG-end4".
       procedure division.
      *Now same tests with instructions
           MOVE "text5-AAAAABBBBBCCCCCDDDDDEEEEEFFFFFGGGGGHHHHHIIIIIJJJJ
      -    "J-end5" TO var1.
      
           MOVE "text6-AAAAABBBBBCCCCCDDDDDEEEEEFFFFFGGGGGHHHHHIIIIIJJJJ
      *always add comments, it helps the devs (but not the parser ;-))
      -    "J-end6" TO var2.
      
           MOVE "text7-AAAAABBBBBCCCCCDDDDDEEEEEFFFFFGGGGGHHHHHIIIIIJJJJ
      
      -    "J-end7" TO var3.
      
           MOVE "text8-AAAAABBBBBCCCCCDDDDDEEEEEFFFFFGGGGGHHHHHIIIIIJJJJ
      -
      -    "J-end8" TO var4.
           goback
           .
       end program DVZZMFT0.