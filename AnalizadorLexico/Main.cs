using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

class Program{

	enum TipoToken{
		TK_PALABRA_RESERVADA, TK_LLAVEI, TK_LLAVED,  TK_IDENTIFICADOR, TK_COMA, TK_PUNTOYCOMA, TK_ASIGNACION, TK_NUMERO, 
		TK_PARI, TK_PARD, TK_MENOR, TK_MAYOR, TK_RESTA, TK_SUMA, TK_MAYORIGUAL, TK_MENORIGUAL, TK_DIVISION, TK_MULTIPLICACION, TK_ERROR,
		TK_CDIFERENTE, TK_BLOQUE_C_I, TK_BLOQUE_C_D, TK_COMENTARIO_LINEA, TK_CIGUALDAD, TK_EOF
	}

	enum Estados{
		INICIO, ID, NUM, PARI,  PARD,  PUNTOYCOMA, COMA , ASIGNAR,  SUMAR, RESTAR, IN_EOF, ERROR,   FIN,
		BLOQUE_C_I, BLOQUE_C_D, MULTIPLICAR, DIVIDIR, CMENOR, CMAYOR, CDIFERENCIAR, COMENTARIO_LINEA, CDIFERENCIAR_IGUAL
	}

	class Token{
		static public TipoToken tt;
		static public string lexema = "";
	}
	
	static string linea = "";
	static char [] cadena;
	static int index = 0, numLinea = 1;
	static char c;
	static string []simbolos_especiales = {"+", "-", "*", "/", "<", "<=", ">", ">=", "==", "!=", "=", ";", ",", "(", ")", "{", "}", "/*", "*/", "//"};
	static string []palabras_reservadas = { "if", "then", "else", "fi", "do", "until", "while", "read", "write", "float", "int", "bool", "program"};
	static char simboloTemporal;
	static char []simbolos_divided = {'+', '-', '*', '/', '<', '=', '>', '!', ';', '(', ')', '{', '}' };
	
	public static void Main(string[] args){
		if (args.Length > 0) {
			lectura (args [0]);
		} else {
			lectura (@"");
		}
	}
	
	static void lectura(string file_path){
		if (file_path.Equals ("")) {
			return;
		}
		StreamReader file = new StreamReader(@file_path);
		while( ( linea = file.ReadLine() ) != null ){
			cadena = linea.ToCharArray(); 
			Console.WriteLine("Linea "+numLinea);
			/*if(numLinea == 8){
				foreach( char ttt in cadena)
					Console.WriteLine("_"+ttt+"_");
			}*/
			while( index < cadena.Length ){
				analizador();
				if( !Token.lexema.Equals("")){
					Console.WriteLine("( "+Token.tt+", "+Token.lexema+" )");
					Token.lexema = "";
				}
			}
			numLinea++;
			index = 0;
		}
	}
	
	static void analizador(){
		Estados estado = Estados.INICIO;
		while( estado != Estados.FIN ){
			switch( estado ){
			case Estados.INICIO:
				c = getChar();
				saltar();
				if( char.IsLetter(c) || c.Equals('_') ){
					estado = Estados.ID;
					Token.lexema += c;
				}else if( char.IsDigit(c) ){ //|| c.Equals('+') || c.Equals('-') ){//positivos o negativos
					estado = Estados.NUM;
					Token.lexema += c;
				}else if( c.Equals('+')){
					Token.tt = TipoToken.TK_SUMA;
					estado = Estados.FIN;
					Token.lexema += c;
				}else if( c.Equals('-')){
					Token.tt = TipoToken.TK_RESTA;
					estado = Estados.FIN;
					Token.lexema += c;
				}else if( c.Equals('*')){
					Token.tt = TipoToken.TK_MULTIPLICACION;
					estado = Estados.MULTIPLICAR;
					Token.lexema += c; 
				}else if( c.Equals('/')){
					Token.tt = TipoToken.TK_DIVISION;
					estado = Estados.DIVIDIR;
					Token.lexema += c;
				}else if( c.Equals(';')){
					Token.tt = TipoToken.TK_PUNTOYCOMA;
					estado = Estados.FIN;
					Token.lexema += c;
				}else if( c.Equals(',')){
					Token.tt = TipoToken.TK_COMA;
					estado = Estados.FIN;
					Token.lexema += c;
				}else if( c.Equals('(')){
					Token.tt = TipoToken.TK_PARI;
					estado = Estados.FIN;
					Token.lexema += c;
				}else if( c.Equals(')')){
					Token.tt = TipoToken.TK_PARD;
					estado = Estados.FIN;
					Token.lexema += c;
				}else if( c.Equals('{')){
					Token.tt = TipoToken.TK_LLAVEI;
					estado = Estados.FIN;
					Token.lexema += c;
				}else if( c.Equals('}')){
					Token.tt = TipoToken.TK_LLAVED;
					estado = Estados.FIN;
					Token.lexema += c;
				}else if( c.Equals('=')){
					Token.tt = TipoToken.TK_ASIGNACION;
					estado = Estados.ASIGNAR;
					Token.lexema += c;
				}else if( c.Equals('<')){
					Token.tt = TipoToken.TK_MENOR;
					estado = Estados.CMENOR;
					Token.lexema += c;
				}else if( c.Equals('>')){
					Token.tt = TipoToken.TK_MAYOR;
					estado = Estados.CMAYOR;
					Token.lexema += c;
				}else if( c.Equals('!')){
					Token.tt = TipoToken.TK_CDIFERENTE;
					estado = Estados.CDIFERENCIAR;
					Token.lexema += c;
				}else if ( c.Equals(null) ) {
					Token.tt = TipoToken.TK_EOF;
					estado = Estados.FIN;
					Token.lexema += c;
				}else {
					Token.tt = TipoToken.TK_ERROR;
					estado = Estados.FIN;
				}
				simboloTemporal = c;
				break;
			case Estados.ID://completar identificador o pr
				try{
				c = getChar();
				if( !(char.IsLetterOrDigit(c) || c.Equals('_') ) ){
					Token.tt = TipoToken.TK_IDENTIFICADOR;
					estado = Estados.FIN;
					buscarPR();
					index--;//modificado
				}else
					Token.lexema += c;
				finToken();
				}catch(IndexOutOfRangeException ex){
					Token.tt = TipoToken.TK_IDENTIFICADOR;
					estado = Estados.FIN;
					buscarPR();
				}
				break;
			case Estados.NUM:
				c = getChar();
				if( !(char.IsDigit(c)) ){
					Token.tt = TipoToken.TK_NUMERO;
					estado = Estados.FIN;
					index--;//modificado
				}else
					Token.lexema += c;
				finToken();
				break;
			case Estados.MULTIPLICAR: 
				c = getChar();
				if( simboloTemporal.Equals('*') && c.Equals('/') ){
					Token.tt = TipoToken.TK_BLOQUE_C_D;
					estado = Estados.FIN;
					Token.lexema += c;
				}else{
					Token.tt = TipoToken.TK_MULTIPLICACION;
					estado = Estados.FIN;
					Token.lexema = "*";
					index--;
				}
				break;
			case Estados.DIVIDIR: 
				c = getChar();
				if( simboloTemporal.Equals('/') && c.Equals('*') ){
					Token.tt = TipoToken.TK_BLOQUE_C_I;
					estado = Estados.BLOQUE_C_I;
					Token.lexema = "";
					simboloTemporal = c;
				}else if( simboloTemporal.Equals('/') && c.Equals('/') ){
					Token.tt = TipoToken.TK_COMENTARIO_LINEA;
					estado = Estados.COMENTARIO_LINEA;
					Token.lexema += c;
					//index--;
				}else{
					Token.tt = TipoToken.TK_DIVISION;
					estado = Estados.FIN;
					Token.lexema = "/";
					index--;
				}
				break;
			case Estados.ASIGNAR: 
				c = getChar();
				if( simboloTemporal.Equals('=') && c.Equals('=') ){
					Token.tt = TipoToken.TK_CIGUALDAD;
					estado = Estados.FIN;
					Token.lexema += c;
				}else{
					Token.tt = TipoToken.TK_ASIGNACION;
					estado = Estados.FIN;
					Token.lexema = "=";
					index--;//---------------------------------------error 1?
				}
				break;
			case Estados.CMENOR: 
				c = getChar();
				if( simboloTemporal.Equals('<') && c.Equals('=') ){
					Token.tt = TipoToken.TK_MENORIGUAL;
					estado = Estados.FIN;
					Token.lexema += c;
				}else{
					Token.tt = TipoToken.TK_MENOR;
					estado = Estados.FIN;
					Token.lexema = "<";
					index--;//---------------------------------------error 2?
				}
				break;
			case Estados.CMAYOR: 
				c = getChar();
				if( simboloTemporal.Equals('>') && c.Equals('=') ){
					Token.tt = TipoToken.TK_MAYORIGUAL;
					estado = Estados.FIN;
					Token.lexema += c;
				}else{
					Token.tt = TipoToken.TK_MAYOR;
					estado = Estados.FIN;
					Token.lexema = ">";
					index--;
				}
				break;
			case Estados.CDIFERENCIAR: 
				c = getChar();
				if( simboloTemporal.Equals('!') && c.Equals('=') ){
					Token.tt = TipoToken.TK_CDIFERENTE;
					estado = Estados.FIN;
					Token.lexema += c;
				}else{
					Token.tt = TipoToken.TK_ERROR;
					estado = Estados.FIN;
					Token.lexema = "!";
					index--;
				}
				break;
			case Estados.COMENTARIO_LINEA:
				if(index == cadena.Length){
					estado = Estados.FIN;
					Token.tt = TipoToken.TK_COMENTARIO_LINEA;
					Token.lexema = "";
					break;
				}
				break;
			case Estados.BLOQUE_C_I:
				c = getChar();
				if(index == cadena.Length){
					estado = Estados.BLOQUE_C_D;
				}
				if(simboloTemporal.Equals('*') && c.Equals('/')){
					estado = Estados.FIN;
					Token.tt = TipoToken.TK_BLOQUE_C_I;
				}else{
					simboloTemporal = c;
				}
				break;
			default: //si no entra en las demas categorias entonces es un error
				Token.tt = TipoToken.TK_ERROR; 
				estado = Estados.FIN; 
				Token.lexema += c;
				break;
			}
			index++;
		}
	}
	static void finToken(){
		//if( char.IsPunctuation(c) || char.IsSeparator(c) || char.IsSymbol(c))
		foreach( char temp in simbolos_divided){
			if( temp.Equals(c) ){
				//index--; break;	
			}
		}
	}
	static void buscarPR(){
		foreach( string pr in palabras_reservadas ){
			if( Token.lexema.Equals(pr) ){
				Token.lexema = pr;
				Token.tt = TipoToken.TK_PALABRA_RESERVADA; break;
			}
		}
	}
	static void saltar(){
		while( esDelimitador( c ) ){
			index++;
			c = getChar();
		}
	}
	static bool esDelimitador( char c ){
		if( c.Equals(' ') || c.Equals('\n') || c.Equals('\t') )return true;
		else return false;
	}
	static char getChar(){
		if(index>=cadena.Length){
			return '|';
		}
		return cadena[index];
	}
}
