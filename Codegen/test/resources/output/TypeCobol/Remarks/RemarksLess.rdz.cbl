﻿000000 IDENTIFICATION DIVISION.
000000 PROGRAM-ID. RemarksLess. 
      *REMARKS. COPY=(
      *        YPRTD20L
      *        YCONVEAL
      *        ).
                                                                                                                
000000 ENVIRONMENT DIVISION.
000000 DATA DIVISION.
000000 WORKING-STORAGE section.
000000    01 PRTD20L. COPY  YPRTD20L.   
000000    01 CONVEAL. COPY  YCONVEAL.   
000000    
000000
000000 PROCEDURE DIVISION.
000000     GOBACK
000000     .
000000 END PROGRAM RemarksLess.
