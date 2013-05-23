using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

class AnalizadorLexico{

	public enum TipoToken{
		TK_PALABRA_RESERVADA, TK_LLAVEI, TK_LLAVED,  TK_IDENTIFICADOR, TK_COMA, TK_PUNTOYCOMA, TK_ASIGNACION, TK_NUMERO, 
		TK_PARI, TK_PARD, TK_MENOR, TK_MAYOR, TK_RESTA, TK_SUMA, TK_MAYORIGUAL, TK_MENORIGUAL, TK_DIVISION, TK_MULTIPLICACION, TK_ERROR,
		TK_CDIFERENTE, TK_BLOQUE_C_I, TK_BLOQUE_C_D, TK_COMENTARIO_LINEA, TK_CIGUALDAD, TK_EOF
	}

	public enum Estados{
		INICIO, ID, NUM, PARI,  PARD,  PUNTOYCOMA, COMA , ASIGNAR,  SUMAR, RESTAR, IN_EOF, ERROR,   FIN,
		BLOQUE_C_I, BLOQUE_C_D, MULTIPLICAR, DIVIDIR, CMENOR, CMAYOR, CDIFERENCIAR, COMENTARIO_LINEA, CDIFERENCIAR_IGUAL
	}

	public class Token{
		public TipoToken tt;
		public string lexema = "";
	}

	private string source = "";
	private StreamReader file = null;
	private string linea = "";
	private char [] cadena;
	private int index = 0, numLinea = 1;
	private char c;
	private string []palabras_reservadas = { "if", "then", "else", "fi", "do", "until", "while", "read", "write", "float", "int", "bool", "program"};
	private char simboloTemporal;
	private char []simbolos_divided = {'+', '-', '*', '/', '<', '=', '>', '!', ';', '(', ')', '{', '}' };

	public AnalizadorLexico(string src){
		this.source = src;
		this.file 	= new StreamReader (this.source);
		this.linea 	= file.ReadLine ();
		this.cadena = this.linea.ToCharArray ();
	}

	public void lectura(){
		while( linea != null ){
			cadena = linea.ToCharArray();
			Token tk = new Token ();
			while( index < cadena.Length ){
				tk = analizador();
				if( !tk.lexema.Equals("")){
					Console.WriteLine("( "+tk.tt+", "+tk.lexema+" )");
					tk.lexema = "";
				}
			}
			numLinea++;
			index = 0;
			linea = file.ReadLine ();
		}
	}

	public Token analizador(){
		Token t = new Token ();
		if (index < cadena.Length) {
			Estados estado = Estados.INICIO;
			while (estado != Estados.FIN) {
				switch (estado) {
				case Estados.INICIO:
					c = getChar ();
					saltar ();
					if (char.IsLetter (c) || c.Equals ('_')) {
						estado = Estados.ID;
						t.lexema += c;
					} else if (char.IsDigit (c)) { //|| c.Equals('+') || c.Equals('-') ){//positivos o negativos
						estado = Estados.NUM;
						t.lexema += c;
					} else if (c.Equals ('+')) {
						t.tt = TipoToken.TK_SUMA;
						estado = Estados.FIN;
						t.lexema += c;
					} else if (c.Equals ('-')) {
						t.tt = TipoToken.TK_RESTA;
						estado = Estados.FIN;
						t.lexema += c;
					} else if (c.Equals ('*')) {
						t.tt = TipoToken.TK_MULTIPLICACION;
						estado = Estados.MULTIPLICAR;
						t.lexema += c; 
					} else if (c.Equals ('/')) {
						t.tt = TipoToken.TK_DIVISION;
						estado = Estados.DIVIDIR;
						t.lexema += c;
					} else if (c.Equals (';')) {
						t.tt = TipoToken.TK_PUNTOYCOMA;
						estado = Estados.FIN;
						t.lexema += c;
					} else if (c.Equals (',')) {
						t.tt = TipoToken.TK_COMA;
						estado = Estados.FIN;
						t.lexema += c;
					} else if (c.Equals ('(')) {
						t.tt = TipoToken.TK_PARI;
						estado = Estados.FIN;
						t.lexema += c;
					} else if (c.Equals (')')) {
						t.tt = TipoToken.TK_PARD;
						estado = Estados.FIN;
						t.lexema += c;
					} else if (c.Equals ('{')) {
						t.tt = TipoToken.TK_LLAVEI;
						estado = Estados.FIN;
						t.lexema += c;
					} else if (c.Equals ('}')) {
						t.tt = TipoToken.TK_LLAVED;
						estado = Estados.FIN;
						t.lexema += c;
					} else if (c.Equals ('=')) {
						t.tt = TipoToken.TK_ASIGNACION;
						estado = Estados.ASIGNAR;
						t.lexema += c;
					} else if (c.Equals ('<')) {
						t.tt = TipoToken.TK_MENOR;
						estado = Estados.CMENOR;
						t.lexema += c;
					} else if (c.Equals ('>')) {
						t.tt = TipoToken.TK_MAYOR;
						estado = Estados.CMAYOR;
						t.lexema += c;
					} else if (c.Equals ('!')) {
						t.tt = TipoToken.TK_CDIFERENTE;
						estado = Estados.CDIFERENCIAR;
						t.lexema += c;
					} else if (c.Equals (null)) {
						t.tt = TipoToken.TK_EOF;
						estado = Estados.FIN;
						t.lexema += c;
					} else {
						t.tt = TipoToken.TK_ERROR;
						estado = Estados.FIN;
					}
					simboloTemporal = c;
					break;
				case Estados.ID://completar identificador o pr
					try {
						c = getChar ();
						if (!(char.IsLetterOrDigit (c) || c.Equals ('_'))) {
							t.tt = TipoToken.TK_IDENTIFICADOR;
							estado = Estados.FIN;
							buscarPR (t);
							index--;//modificado
						} else
							t.lexema += c;
						finToken ();
					} catch (IndexOutOfRangeException) {
						t.tt = TipoToken.TK_IDENTIFICADOR;
						estado = Estados.FIN;
						buscarPR (t);
					}
					break;
				case Estados.NUM:
					c = getChar ();
					if (!(char.IsDigit (c))) {
						t.tt = TipoToken.TK_NUMERO;
						estado = Estados.FIN;
						index--;//modificado
					} else
						t.lexema += c;
					finToken ();
					break;
				case Estados.MULTIPLICAR: 
					c = getChar ();
					if (simboloTemporal.Equals ('*') && c.Equals ('/')) {
						t.tt = TipoToken.TK_BLOQUE_C_D;
						estado = Estados.FIN;
						t.lexema += c;
					} else {
						t.tt = TipoToken.TK_MULTIPLICACION;
						estado = Estados.FIN;
						t.lexema = "*";
						index--;
					}
					break;
				case Estados.DIVIDIR: 
					c = getChar ();
					if (simboloTemporal.Equals ('/') && c.Equals ('*')) {
						t.tt = TipoToken.TK_BLOQUE_C_I;
						estado = Estados.BLOQUE_C_I;
						t.lexema = "";
						simboloTemporal = c;
					} else if (simboloTemporal.Equals ('/') && c.Equals ('/')) {
						t.tt = TipoToken.TK_COMENTARIO_LINEA;
						estado = Estados.COMENTARIO_LINEA;
						t.lexema += c;
						//index--;
					} else {
						t.tt = TipoToken.TK_DIVISION;
						estado = Estados.FIN;
						t.lexema = "/";
						index--;
					}
					break;
				case Estados.ASIGNAR: 
					c = getChar ();
					if (simboloTemporal.Equals ('=') && c.Equals ('=')) {
						t.tt = TipoToken.TK_CIGUALDAD;
						estado = Estados.FIN;
						t.lexema += c;
					} else {
						t.tt = TipoToken.TK_ASIGNACION;
						estado = Estados.FIN;
						t.lexema = "=";
						index--;//---------------------------------------error 1?
					}
					break;
				case Estados.CMENOR: 
					c = getChar ();
					if (simboloTemporal.Equals ('<') && c.Equals ('=')) {
						t.tt = TipoToken.TK_MENORIGUAL;
						estado = Estados.FIN;
						t.lexema += c;
					} else {
						t.tt = TipoToken.TK_MENOR;
						estado = Estados.FIN;
						t.lexema = "<";
						index--;//---------------------------------------error 2?
					}
					break;
				case Estados.CMAYOR: 
					c = getChar ();
					if (simboloTemporal.Equals ('>') && c.Equals ('=')) {
						t.tt = TipoToken.TK_MAYORIGUAL;
						estado = Estados.FIN;
						t.lexema += c;
					} else {
						t.tt = TipoToken.TK_MAYOR;
						estado = Estados.FIN;
						t.lexema = ">";
						index--;
					}
					break;
				case Estados.CDIFERENCIAR: 
					c = getChar ();
					if (simboloTemporal.Equals ('!') && c.Equals ('=')) {
						t.tt = TipoToken.TK_CDIFERENTE;
						estado = Estados.FIN;
						t.lexema += c;
					} else {
						t.tt = TipoToken.TK_ERROR;
						estado = Estados.FIN;
						t.lexema = "!";
						index--;
					}
					break;
				case Estados.COMENTARIO_LINEA:
					if (index == cadena.Length) {
						estado = Estados.FIN;
						t.tt = TipoToken.TK_COMENTARIO_LINEA;
						t.lexema = "";
						break;
					}
					break;
				case Estados.BLOQUE_C_I:
					c = getChar ();
					if (index == cadena.Length) {
						estado = Estados.BLOQUE_C_D;
					}
					if (simboloTemporal.Equals ('*') && c.Equals ('/')) {
						estado = Estados.FIN;
						t.tt = TipoToken.TK_BLOQUE_C_I;
					} else {
						simboloTemporal = c;
					}
					break;
				default: //si no entra en las demas categorias entonces es un error
					t.tt = TipoToken.TK_ERROR; 
					estado = Estados.FIN; 
					t.lexema += c;
					break;
				}
				index++;
			}
		} else {
			linea = file.ReadLine ();
			if (linea != null) {
				cadena = linea.ToCharArray ();
				numLinea++;
				index = 0;
				t.lexema = "";
			} else {
				t.tt = TipoToken.TK_EOF;
			}
		}
		return t;
	}
	private void finToken(){
		//if( char.IsPunctuation(c) || char.IsSeparator(c) || char.IsSymbol(c))
		foreach( char temp in simbolos_divided){
			if( temp.Equals(c) ){
				//index--; break;	
			}
		}
	}
	private void buscarPR(Token t){
		foreach( string pr in palabras_reservadas ){
			if( t.lexema.Equals(pr) ){
				t.lexema = pr;
				t.tt = TipoToken.TK_PALABRA_RESERVADA; break;
			}
		}
	}
	private void saltar(){
		while( esDelimitador( c ) ){
			index++;
			c = getChar();
		}
	}
	private bool esDelimitador( char c ){
		if( c.Equals(' ') || c.Equals('\n') || c.Equals('\t') )return true;
		else return false;
	}
	private char getChar(){
		if(index>=cadena.Length){
			return '|';
		}
		return cadena[index];
	}
}
