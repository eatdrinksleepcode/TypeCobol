﻿       IDENTIFICATION DIVISION.
       PROGRAM-ID. Pgm.
       DATA DIVISION.
           REPLACE
             ==:StrtPgm:== By
             ==
               SET MyVar1 TO True
               SET MyVar2 TO True
             ==
           .
       WORKING-STORAGE SECTION.
       01 item PIC X.
          88 MyVar1 VALUE 'A'.
          88 MyVar2 VALUE 'B'.
       PROCEDURE DIVISION.
             :StrtPgm:
             GOBACK
             .
       END Program Pgm.