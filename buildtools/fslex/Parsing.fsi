//==========================================================================
// (c) Microsoft Corporation 2005-2009.
//=========================================================================

namespace FSharp.Text.Parsing
open FSharp.Text.Lexing

open System.Collections.Generic

/// The information accessible via the <c>parseState</c> value within parser actions.
type IParseState = 
    /// Get the start and end position for the terminal or non-terminal at a given index matched by the production
    abstract InputRange: index:int -> Position * Position

    /// Get the end position for the terminal or non-terminal at a given index matched by the production
    abstract InputEndPosition: int -> Position 

    /// Get the start position for the terminal or non-terminal at a given index matched by the production
    abstract InputStartPosition: int -> Position 

    /// Get the full range of positions matched by the production
    abstract ResultRange: Position * Position

    /// Get the value produced by the terminal or non-terminal at the given position
    abstract GetInput   : int -> obj 

    /// Get the store of local values associated with this parser
    // Dynamically typed, non-lexically scoped local store
    abstract ParserLocalStore : IDictionary<string,obj>

    /// Raise an error in this parse context
    abstract RaiseError<'b> : unit -> 'b 


[<Sealed>]
/// The context provided when a parse error occurs
type ParseErrorContext<'tok> =
      /// The stack of state indexes active at the parse error 
      member StateStack  : int list

      /// The state active at the parse error 
      member ParseState : IParseState

      /// The tokens that would cause a reduction at the parse error 
      member ReduceTokens: int list

      /// The stack of productions that would be reduced at the parse error 
      member ReducibleProductions : int list list

      /// The token that caused the parse error
      member CurrentToken : 'tok option

      /// The token that would cause a shift at the parse error
      member ShiftTokens : int list

      /// The message associated with the parse error
      member Message : string

/// Tables generated by fsyacc
/// The type of the tables contained in a file produced by the fsyacc.exe parser generator.
type Tables<'tok> = 
    { 
      /// The reduction table
      reductions: (IParseState -> obj) array ;
      
      /// The token number indicating the end of input
      endOfInputTag: int;
      
      /// A function to compute the tag of a token
      tagOfToken: 'tok -> int;
      
      /// A function to compute the data carried by a token
      dataOfToken: 'tok -> obj; 
      
      /// The sparse action table elements
      actionTableElements: uint16[];
      
      /// The sparse action table row offsets
      actionTableRowOffsets: uint16[];
      
      /// The number of symbols for each reduction
      reductionSymbolCounts: uint16[];
      
      /// The immediate action table
      immediateActions: uint16[];      
      
      /// The sparse goto table
      gotos: uint16[];
      
      /// The sparse goto table row offsets
      sparseGotoTableRowOffsets: uint16[];
      
      /// The sparse table for the productions active for each state
      stateToProdIdxsTableElements: uint16[];  
      
      /// The sparse table offsets for the productions active for each state
      stateToProdIdxsTableRowOffsets: uint16[];  
      
      /// This table is logically part of the Goto table
      productionToNonTerminalTable: uint16[];
      
      /// This function is used to hold the user specified "parse_error" or "parse_error_rich" functions
      parseError:  ParseErrorContext<'tok> -> unit;
      
      /// The total number of terminals 
      numTerminals: int;
      
      /// The tag of the error terminal
      tagOfErrorTerminal: int 
    }

    /// Interpret the parser table taking input from the given lexer, using the given lex buffer, and the given start state.
    /// Returns an object indicating the final synthesized value for the parse.
    member Interpret :  lexer:(LexBuffer<'char> -> 'tok) * lexbuf:LexBuffer<'char> * startState:int -> obj 

/// Indicates an accept action has occured
exception Accept of obj
/// Indicates a parse error has occured and parse recovery is in progress
exception RecoverableParseError

#if __DEBUG
module internal Flags =
  val mutable debug : bool
#endif

/// Helpers used by generated parsers.
module ParseHelpers = 
   /// The default implementation of the parse_error_rich function
   val parse_error_rich: (ParseErrorContext<'tok> -> unit) option
   
   /// The default implementation of the parse_error function
   val parse_error: string -> unit
