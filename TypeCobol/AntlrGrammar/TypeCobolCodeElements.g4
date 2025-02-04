grammar TypeCobolCodeElements;

import Cobol2002CodeElements;

// --- Starting rule ---
cobolCodeElements: codeElement* EOF;

tcCodeElement:
	  functionDeclarationHeader
	| functionDeclarationEnd
	| libraryCopy
	| globalStorageSectionHeader
	;


// See Issue #805 GLOBAL-STORAGE SECTION
// This is the rule for Global-Storage Section Header CodeElement. 
// Rule - GLOBALSS
globalStorageSectionHeader: 
	GLOBAL_STORAGE SECTION PeriodSeparator;

qualifiedParagraphNameReference: cobolQualifiedParagraphNameReference | tcQualifiedParagraphNameReference;
cobolQualifiedParagraphNameReference: // was qualifiedParagraphNameReference
	paragraphNameReference (IN | OF) sectionNameReference;
tcQualifiedParagraphNameReference: // new feature
	sectionNameReference QualifiedNameSeparator paragraphNameReference;

qualifiedDataName1: cobolQualifiedDataName1 | tcQualifiedDataName1;
cobolQualifiedDataName1: // was qualifiedDataName1
	dataNameReference ((IN | OF) dataNameReferenceOrFileNameReference)+;
tcQualifiedDataName1: // new feature
	(dataNameReferenceOrFileNameReference QualifiedNameSeparator)+ dataNameReference;

qualifiedConditionName: cobolQualifiedConditionName | tcQualifiedConditionName;
cobolQualifiedConditionName: // was qualifiedConditionName
	conditionNameReferenceOrConditionForUPSISwitchNameReference ((IN | OF) dataNameReferenceOrFileNameReferenceOrMnemonicForUPSISwitchNameReference)*;
tcQualifiedConditionName: // new feature
	(dataNameReferenceOrFileNameReferenceOrMnemonicForUPSISwitchNameReference QualifiedNameSeparator)* conditionNameReferenceOrConditionForUPSISwitchNameReference;

qualifiedDataNameOrQualifiedConditionName1: cobolQualifiedDataNameOrQualifiedConditionName1 | tcQualifiedDataNameOrQualifiedConditionName1;
cobolQualifiedDataNameOrQualifiedConditionName1: // was qualifiedDataNameOrQualifiedConditionName1
	dataNameReferenceOrConditionNameReferenceOrConditionForUPSISwitchNameReference ((IN | OF) dataNameReferenceOrFileNameReferenceOrMnemonicForUPSISwitchNameReference)+;
tcQualifiedDataNameOrQualifiedConditionName1: // new feature
	(dataNameReferenceOrFileNameReferenceOrMnemonicForUPSISwitchNameReference QualifiedNameSeparator)+ dataNameReferenceOrConditionNameReferenceOrConditionForUPSISwitchNameReference;

qualifiedTextName: (textName ((IN | OF) libraryName)?) | (libraryName QualifiedNameSeparator textName);



// rule modified to support:
// - MOVE UNSAFE <custom type> TO <custom type>
// - MOVE TRUE  TO <boolean>
// - MOVE FALSE TO <boolean>
moveSimple: MOVE UNSAFE? (booleanValue | variable7) TO storageArea1+;
//                   ^          ^
//                    \          \
//                     \          --------------  MOVE [TRUE|FALSE] TO <boolean>
//                      >-----------------------  MOVE UNSAFE <custom type> TO <custom type>
//                      \
//                       V
moveCorresponding: MOVE UNSAFE? (CORRESPONDING | CORR) fromGroupItem=dataItemReference TO toGroupItem=dataItemReference;


// rule modified to support : 
// - SET UNSAFE <data> TO <data>
setStatementForAssignment:
	SET UNSAFE? setReceivingField=dataOrIndexStorageArea+ TO setSendingField;


// rule modified to support:
// - SET <boolean> TO FALSE
setStatementForConditions:
	SET conditionStorageArea+ TO (TRUE | FALSE);



libraryCopy: SERVICE ID? IS? qualifiedTextName PeriodSeparator?; // TCRFUN_LIBRARY_COPY



// rules modified to support user defined functions (of arity 0..n)
functionIdentifier: intrinsicFunctionCall | userDefinedFunctionCall;
intrinsicFunctionCall: FUNCTION IntrinsicFunctionName (LeftParenthesisSeparator argument* RightParenthesisSeparator)?; // argument* instead of argument+ to enable good error messages
userDefinedFunctionCall: FUNCTION functionNameReference (LeftParenthesisSeparator argument* RightParenthesisSeparator)?;

// - TCRFUN_NO_DEFAULT_ACCESS_MODIFIER
// - TCRFUN_DOTS
// - TCRFUN_NO_DOT_AFTER_VISIBILITY
// - TCRFUN_PARAMETER_DECLARATION_ORDER
// - TCRFUN_0_TO_N_PARAMETERS (possibly 0 parameters because of "?")
// - TCRFUN_0_TO_1_RETURNING_PARAMETER
//   - possibly 0 parameters because of "?" --> procedure or void-returning function
//   - returningPhrase only allows 1 parameter --> function
// - TCRFUN_DECLARATION_NO_USING
functionDeclarationHeader:
	formalizedComment? DECLARE (FUNCTION|PROCEDURE)? functionNameDefinition (PRIVATE | PUBLIC)? inputPhrase? inoutPhrase? outputPhrase? functionReturningPhrase? PeriodSeparator;

// TCRFUN_0_TO_N_PARAMETERS (1..N parameters because of "+")
inputPhrase:  INPUT  parameterDescription+;
inoutPhrase:  IN_OUT  parameterDescription+;
outputPhrase: OUTPUT parameterDescription+;
functionReturningPhrase: RETURNING parameterDescription;

// parameterDescription is a rule created from dataDescriptionEntry and enforcing rules:
// - TCRFUN_PARAMETER_DESCRIPTION
parameterDescription: functionDataParameter PeriodSeparator?;

functionDataParameter:
	dataNameDefinition QUESTION_MARK? (pictureClause | POINTER | cobol2002TypeClause | FUNCTION_POINTER | PROCEDURE_POINTER)
		blankWhenZeroClause?
		justifiedClause?
		synchronizedClause?
		groupUsageClause?
		signClause?
		tcfuncParameterUsageClause?
		valueClause?
		//No OccursClause because we only allow Level 01		
		;

tcfuncParameterUsageClause:
	                (BINARY NATIVE?) |
					(COMP NATIVE?)   | (COMPUTATIONAL NATIVE?)   |
					(COMP_1 NATIVE?) | (COMPUTATIONAL_1 NATIVE?) |
					(COMP_2 NATIVE?) | (COMPUTATIONAL_2 NATIVE?) |
					(COMP_3 NATIVE?) | (COMPUTATIONAL_3 NATIVE?) |
					(COMP_4 NATIVE?) | (COMPUTATIONAL_4 NATIVE?) |
					(COMP_5 NATIVE?) | (COMPUTATIONAL_5 NATIVE?) |
					(DISPLAY NATIVE?)   |
					(DISPLAY_1 NATIVE?) |
					INDEX |
					(NATIONAL NATIVE?) |
					//not implemented yet
					//(OBJECT REFERENCE classNameReference?) |        
					(PACKED_DECIMAL NATIVE?) |              
					    
					//Try to rewrite a cleaner/simpler grammar so POINTER must be declared at picture or type level.
					//FUNCTION_POINTER and PROCEDURE_POINTER are not implemented yet
				    ;

functionDeclarationEnd: END_DECLARE PeriodSeparator;



callStatement:  tcCallStatement | cobolCallStatement;

cobolCallStatement:
	CALL programNameOrProgramEntryOrProcedurePointerOrFunctionPointerVariable?
		(USING callUsingParameters+)?
		(RETURNING callReturningParameter)?;

// TCRFUN_CALL_PARAMETER_ORDER
tcCallStatement:
	CALL procedurePointerOrFunctionPointerVariableOrfunctionNameReference=identifier
		(INPUT  callInputParameter+)?
		(IN_OUT  callInoutParameter+)?
		(OUTPUT callOutputParameter+)?
	;
//procedurePointerOrFunctionPointerVariableOrfunctionNameReference is only used in this rule so do not create a grammar rule for it

callInputParameter:  (BY? (REFERENCE | CONTENT | VALUE))? (sharedVariableOrFileName | OMITTED); // TCRFUN_INPUT_BY
callInoutParameter:  (sharedStorageArea1 | OMITTED);  // TCRFUN_CALL_INOUT_AND_OUTPUT_BY_REFERENCE
callOutputParameter: (sharedStorageArea1 | OMITTED); // TCRFUN_CALL_INOUT_AND_OUTPUT_BY_REFERENCE

typeNameReference: (UserDefinedWord | DATE | CURRENCY);

// When this clause is matched, dataNameDefinition above is also a dataTypeNameDefinition
cobol2002TypedefClause: TYPEDEF (STRICT | STRONG)? (PRIVATE | PUBLIC)?;
cobol2002TypeClause:    TYPE (programNameReference3 QualifiedNameSeparator)? typeNameReference;

//Similar to valueClause but add the possibility to set value as a boolean (TRUE/FALSE)
valueClauseWithBoolean:
	VALUE (value2 | booleanValue);


	
dataDescriptionEntry:
	formalizedComment?
	(
		( { CurrentToken.Text != "66" && CurrentToken.Text != "88" }? 
			levelNumber=integerValue2 (dataNameDefinition | FILLER)? redefinesClause? cobol2002TypedefClause?
			( pictureClause
			| blankWhenZeroClause
			| externalClause
			| globalClause
			| justifiedClause
			| groupUsageClause
			| occursClause
			| signClause
			| synchronizedClause
			| usageClause
			| valueClause
			| (cobol2002TypeClause valueClauseWithBoolean?)
			)* PeriodSeparator
		)
		| dataRenamesEntry
		| dataConditionEntry
	);

setStatementForIndexes:
	SET indexStorageArea+ (UP | DOWN) BY variableOrExpression2;

// ------ Formalized Comment specific ------ 
 
formalizedCommentParam: 
      FORMALIZED_COMMENTS_DESCRIPTION 
    | FORMALIZED_COMMENTS_PARAMETERS 
    | FORMALIZED_COMMENTS_DEPRECATED 
    | FORMALIZED_COMMENTS_REPLACED_BY 
    | FORMALIZED_COMMENTS_RESTRICTION 
    | FORMALIZED_COMMENTS_NEED 
    | FORMALIZED_COMMENTS_SEE 
    | FORMALIZED_COMMENTS_TODO; 
 
formalizedCommentOuterLevel:
	AT_SIGN (formalizedCommentParam ColonSeparator? FORMALIZED_COMMENTS_VALUE?);

formalizedCommentInnerLevel:
	MinusOperator ((UserDefinedWord ColonSeparator? FORMALIZED_COMMENTS_VALUE) | listItemValue=FORMALIZED_COMMENTS_VALUE);

formalizedCommentLine: 
    formalizedCommentOuterLevel | formalizedCommentInnerLevel | FORMALIZED_COMMENTS_VALUE; 
 
formalizedComment: 
    FORMALIZED_COMMENTS_START formalizedCommentLine* FORMALIZED_COMMENTS_STOP; 



procedureDivisionHeader: 
	formalizedComment? PROCEDURE DIVISION usingPhrase? returningPhrase? PeriodSeparator;