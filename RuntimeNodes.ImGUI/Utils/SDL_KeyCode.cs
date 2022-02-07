using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes.ImGUI.Utils
{
    public static class SDL_KeyCode_Utils
    {
        public const int SDLK_SCANCODE_MASK = 1<<30;
        public static int SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode X) => ((int)X | SDLK_SCANCODE_MASK);
    }

    public static class SDL_KeyCode
    {
        public const int SDLK_UNKNOWN = 0;

        public const int SDLK_RETURN = '\r';
        public const int SDLK_ESCAPE = '\x1B';
        public const int SDLK_BACKSPACE = '\b';
        public const int SDLK_TAB = '\t';
        public const int SDLK_SPACE = ' ';
        public const int SDLK_EXCLAIM = '!';
        public const int SDLK_QUOTEDBL = '"';
        public const int SDLK_HASH = '#';
        public const int SDLK_PERCENT = '%';
        public const int SDLK_DOLLAR = '$';
        public const int SDLK_AMPERSAND = '&';
        public const int SDLK_QUOTE = '\'';
        public const int SDLK_LEFTPAREN = '(';
        public const int SDLK_RIGHTPAREN = ')';
        public const int SDLK_ASTERISK = '*';
        public const int SDLK_PLUS = '+';
        public const int SDLK_COMMA = ';';
        public const int SDLK_MINUS = '-';
        public const int SDLK_PERIOD = '.';
        public const int SDLK_SLASH = '/';
        public const int SDLK_0 = '0';
        public const int SDLK_1 = '1';
        public const int SDLK_2 = '2';
        public const int SDLK_3 = '3';
        public const int SDLK_4 = '4';
        public const int SDLK_5 = '5';
        public const int SDLK_6 = '6';
        public const int SDLK_7 = '7';
        public const int SDLK_8 = '8';
        public const int SDLK_9 = '9';
        public const int SDLK_COLON = ':';
        public const int SDLK_SEMICOLON = ';';
        public const int SDLK_LESS = '<';
        public const int SDLK_EQUALS = '=';
        public const int SDLK_GREATER = '>';
        public const int SDLK_QUESTION = '?';
        public const int SDLK_AT = '@';
        /*
           Skip uppercase letters
         */
        public const int SDLK_LEFTBRACKET = '[';
        public const int SDLK_BACKSLASH = '\\';
        public const int SDLK_RIGHTBRACKET = ']';
        public const int SDLK_CARET = '^';
        public const int SDLK_UNDERSCORE = '_';
        public const int SDLK_BACKQUOTE = '`';
        public const int SDLK_a = 'a';
        public const int SDLK_b = 'b';
        public const int SDLK_c = 'c';
        public const int SDLK_d = 'd';
        public const int SDLK_e = 'e';
        public const int SDLK_f = 'f';
        public const int SDLK_g = 'g';
        public const int SDLK_h = 'h';
        public const int SDLK_i = 'i';
        public const int SDLK_j = 'j';
        public const int SDLK_k = 'k';
        public const int SDLK_l = 'l';
        public const int SDLK_m = 'm';
        public const int SDLK_n = 'n';
        public const int SDLK_o = 'o';
        public const int SDLK_p = 'p';
        public const int SDLK_q = 'q';
        public const int SDLK_r = 'r';
        public const int SDLK_s = 's';
        public const int SDLK_t = 't';
        public const int SDLK_u = 'u';
        public const int SDLK_v = 'v';
        public const int SDLK_w = 'w';
        public const int SDLK_x = 'x';
        public const int SDLK_y = 'y';
        public const int SDLK_z = 'z';

        public static readonly int SDLK_CAPSLOCK = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_CAPSLOCK);

        public static readonly int SDLK_F1 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F1);
        public static readonly int SDLK_F2 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F2);
        public static readonly int SDLK_F3 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F3);
        public static readonly int SDLK_F4 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F4);
        public static readonly int SDLK_F5 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F5);
        public static readonly int SDLK_F6 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F6);
        public static readonly int SDLK_F7 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F7);
        public static readonly int SDLK_F8 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F8);
        public static readonly int SDLK_F9 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F9);
        public static readonly int SDLK_F10 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F10);
        public static readonly int SDLK_F11 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F11);
        public static readonly int SDLK_F12 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F12);

        public static readonly int SDLK_PRINTSCREEN = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_PRINTSCREEN);
        public static readonly int SDLK_SCROLLLOCK = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_SCROLLLOCK);
        public static readonly int SDLK_PAUSE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_PAUSE);
        public static readonly int SDLK_INSERT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_INSERT);
        public static readonly int SDLK_HOME = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_HOME);
        public static readonly int SDLK_PAGEUP = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_PAGEUP);
        public static readonly int SDLK_DELETE = '\x7F';
        public static readonly int SDLK_END = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_END);
        public static readonly int SDLK_PAGEDOWN = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_PAGEDOWN);
        public static readonly int SDLK_RIGHT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_RIGHT);
        public static readonly int SDLK_LEFT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_LEFT);
        public static readonly int SDLK_DOWN = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_DOWN);
        public static readonly int SDLK_UP = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_UP);

        public static readonly int SDLK_NUMLOCKCLEAR = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_NUMLOCKCLEAR);
        public static readonly int SDLK_KP_DIVIDE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_DIVIDE);
        public static readonly int SDLK_KP_MULTIPLY = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_MULTIPLY);
        public static readonly int SDLK_KP_MINUS = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_MINUS);
        public static readonly int SDLK_KP_PLUS = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_PLUS);
        public static readonly int SDLK_KP_ENTER = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_ENTER);
        public static readonly int SDLK_KP_1 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_1);
        public static readonly int SDLK_KP_2 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_2);
        public static readonly int SDLK_KP_3 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_3);
        public static readonly int SDLK_KP_4 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_4);
        public static readonly int SDLK_KP_5 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_5);
        public static readonly int SDLK_KP_6 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_6);
        public static readonly int SDLK_KP_7 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_7);
        public static readonly int SDLK_KP_8 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_8);
        public static readonly int SDLK_KP_9 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_9);
        public static readonly int SDLK_KP_0 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_0);
        public static readonly int SDLK_KP_PERIOD = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_PERIOD);

        public static readonly int SDLK_APPLICATION = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_APPLICATION);
        public static readonly int SDLK_POWER = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_POWER);
        public static readonly int SDLK_KP_EQUALS = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_EQUALS);
        public static readonly int SDLK_F13 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F13);
        public static readonly int SDLK_F14 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F14);
        public static readonly int SDLK_F15 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F15);
        public static readonly int SDLK_F16 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F16);
        public static readonly int SDLK_F17 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F17);
        public static readonly int SDLK_F18 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F18);
        public static readonly int SDLK_F19 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F19);
        public static readonly int SDLK_F20 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F20);
        public static readonly int SDLK_F21 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F21);
        public static readonly int SDLK_F22 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F22);
        public static readonly int SDLK_F23 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F23);
        public static readonly int SDLK_F24 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_F24);
        public static readonly int SDLK_EXECUTE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_EXECUTE);
        public static readonly int SDLK_HELP = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_HELP);
        public static readonly int SDLK_MENU = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_MENU);
        public static readonly int SDLK_SELECT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_SELECT);
        public static readonly int SDLK_STOP = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_STOP);
        public static readonly int SDLK_AGAIN = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AGAIN);
        public static readonly int SDLK_UNDO = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_UNDO);
        public static readonly int SDLK_CUT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_CUT);
        public static readonly int SDLK_COPY = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_COPY);
        public static readonly int SDLK_PASTE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_PASTE);
        public static readonly int SDLK_FIND = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_FIND);
        public static readonly int SDLK_MUTE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_MUTE);
        public static readonly int SDLK_VOLUMEUP = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_VOLUMEUP);
        public static readonly int SDLK_VOLUMEDOWN = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_VOLUMEDOWN);
        public static readonly int SDLK_KP_COMMA = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_COMMA);
        public static readonly int SDLK_KP_EQUALSAS400 =
            SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_EQUALSAS400);

        public static readonly int SDLK_ALTERASE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_ALTERASE);
        public static readonly int SDLK_SYSREQ = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_SYSREQ);
        public static readonly int SDLK_CANCEL = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_CANCEL);
        public static readonly int SDLK_CLEAR = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_CLEAR);
        public static readonly int SDLK_PRIOR = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_PRIOR);
        public static readonly int SDLK_RETURN2 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_RETURN2);
        public static readonly int SDLK_SEPARATOR = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_SEPARATOR);
        public static readonly int SDLK_OUT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_OUT);
        public static readonly int SDLK_OPER = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_OPER);
        public static readonly int SDLK_CLEARAGAIN = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_CLEARAGAIN);
        public static readonly int SDLK_CRSEL = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_CRSEL);
        public static readonly int SDLK_EXSEL = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_EXSEL);

        public static readonly int SDLK_KP_00 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_00);
        public static readonly int SDLK_KP_000 = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_000);
        public static readonly int SDLK_THOUSANDSSEPARATOR =
            SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_THOUSANDSSEPARATOR);
        public static readonly int SDLK_DECIMALSEPARATOR =
            SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_DECIMALSEPARATOR);
        public static readonly int SDLK_CURRENCYUNIT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_CURRENCYUNIT);
        public static readonly int SDLK_CURRENCYSUBUNIT =
            SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_CURRENCYSUBUNIT);
        public static readonly int SDLK_KP_LEFTPAREN = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_LEFTPAREN);
        public static readonly int SDLK_KP_RIGHTPAREN = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_RIGHTPAREN);
        public static readonly int SDLK_KP_LEFTBRACE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_LEFTBRACE);
        public static readonly int SDLK_KP_RIGHTBRACE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_RIGHTBRACE);
        public static readonly int SDLK_KP_TAB = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_TAB);
        public static readonly int SDLK_KP_BACKSPACE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_BACKSPACE);
        public static readonly int SDLK_KP_A = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_A);
        public static readonly int SDLK_KP_B = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_B);
        public static readonly int SDLK_KP_C = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_C);
        public static readonly int SDLK_KP_D = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_D);
        public static readonly int SDLK_KP_E = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_E);
        public static readonly int SDLK_KP_F = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_F);
        public static readonly int SDLK_KP_XOR = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_XOR);
        public static readonly int SDLK_KP_POWER = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_POWER);
        public static readonly int SDLK_KP_PERCENT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_PERCENT);
        public static readonly int SDLK_KP_LESS = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_LESS);
        public static readonly int SDLK_KP_GREATER = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_GREATER);
        public static readonly int SDLK_KP_AMPERSAND = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_AMPERSAND);
        public static readonly int SDLK_KP_DBLAMPERSAND =
            SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_DBLAMPERSAND);
        public static readonly int SDLK_KP_VERTICALBAR =
            SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_VERTICALBAR);
        public static readonly int SDLK_KP_DBLVERTICALBAR =
            SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_DBLVERTICALBAR);
        public static readonly int SDLK_KP_COLON = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_COLON);
        public static readonly int SDLK_KP_HASH = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_HASH);
        public static readonly int SDLK_KP_SPACE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_SPACE);
        public static readonly int SDLK_KP_AT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_AT);
        public static readonly int SDLK_KP_EXCLAM = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_EXCLAM);
        public static readonly int SDLK_KP_MEMSTORE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_MEMSTORE);
        public static readonly int SDLK_KP_MEMRECALL = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_MEMRECALL);
        public static readonly int SDLK_KP_MEMCLEAR = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_MEMCLEAR);
        public static readonly int SDLK_KP_MEMADD = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_MEMADD);
        public static readonly int SDLK_KP_MEMSUBTRACT =
            SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_MEMSUBTRACT);
        public static readonly int SDLK_KP_MEMMULTIPLY =
            SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_MEMMULTIPLY);
        public static readonly int SDLK_KP_MEMDIVIDE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_MEMDIVIDE);
        public static readonly int SDLK_KP_PLUSMINUS = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_PLUSMINUS);
        public static readonly int SDLK_KP_CLEAR = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_CLEAR);
        public static readonly int SDLK_KP_CLEARENTRY = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_CLEARENTRY);
        public static readonly int SDLK_KP_BINARY = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_BINARY);
        public static readonly int SDLK_KP_OCTAL = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_OCTAL);
        public static readonly int SDLK_KP_DECIMAL = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_DECIMAL);
        public static readonly int SDLK_KP_HEXADECIMAL =
            SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KP_HEXADECIMAL);

        public static readonly int SDLK_LCTRL = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_LCTRL);
        public static readonly int SDLK_LSHIFT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_LSHIFT);
        public static readonly int SDLK_LALT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_LALT);
        public static readonly int SDLK_LGUI = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_LGUI);
        public static readonly int SDLK_RCTRL = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_RCTRL);
        public static readonly int SDLK_RSHIFT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_RSHIFT);
        public static readonly int SDLK_RALT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_RALT);
        public static readonly int SDLK_RGUI = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_RGUI);

        public static readonly int SDLK_MODE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_MODE);

        public static readonly int SDLK_AUDIONEXT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AUDIONEXT);
        public static readonly int SDLK_AUDIOPREV = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AUDIOPREV);
        public static readonly int SDLK_AUDIOSTOP = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AUDIOSTOP);
        public static readonly int SDLK_AUDIOPLAY = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AUDIOPLAY);
        public static readonly int SDLK_AUDIOMUTE = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AUDIOMUTE);
        public static readonly int SDLK_MEDIASELECT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_MEDIASELECT);
        public static readonly int SDLK_WWW = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_WWW);
        public static readonly int SDLK_MAIL = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_MAIL);
        public static readonly int SDLK_CALCULATOR = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_CALCULATOR);
        public static readonly int SDLK_COMPUTER = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_COMPUTER);
        public static readonly int SDLK_AC_SEARCH = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AC_SEARCH);
        public static readonly int SDLK_AC_HOME = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AC_HOME);
        public static readonly int SDLK_AC_BACK = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AC_BACK);
        public static readonly int SDLK_AC_FORWARD = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AC_FORWARD);
        public static readonly int SDLK_AC_STOP = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AC_STOP);
        public static readonly int SDLK_AC_REFRESH = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AC_REFRESH);
        public static readonly int SDLK_AC_BOOKMARKS = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_AC_BOOKMARKS);

        public static readonly int SDLK_BRIGHTNESSDOWN =
            SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_BRIGHTNESSDOWN);
        public static readonly int SDLK_BRIGHTNESSUP = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_BRIGHTNESSUP);
        public static readonly int SDLK_DISPLAYSWITCH = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_DISPLAYSWITCH);
        public static readonly int SDLK_KBDILLUMTOGGLE =
            SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KBDILLUMTOGGLE);
        public static readonly int SDLK_KBDILLUMDOWN = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KBDILLUMDOWN);
        public static readonly int SDLK_KBDILLUMUP = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_KBDILLUMUP);
        public static readonly int SDLK_EJECT = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_EJECT);
        public static readonly int SDLK_SLEEP = SDL_KeyCode_Utils.SDL_SCANCODE_TO_KEYCODE(SDL_ScanCode.SDL_SCANCODE_SLEEP);
    }

    public static class SDL_Keymod
    {

        /**
         * \brief Enumeration of valid key mods (possibly OR'd together).
        */
        public const int KMOD_NONE = 0x0000;
        public const int KMOD_LSHIFT = 0x0001;
        public const int KMOD_RSHIFT = 0x0002;
        public const int KMOD_LCTRL = 0x0040;
        public const int KMOD_RCTRL = 0x0080;
        public const int KMOD_LALT = 0x0100;
        public const int KMOD_RALT = 0x0200;
        public const int KMOD_LGUI = 0x0400;
        public const int KMOD_RGUI = 0x0800;
        public const int KMOD_NUM = 0x1000;
        public const int KMOD_CAPS = 0x2000;
        public const int KMOD_MODE = 0x4000;
        public const int KMOD_RESERVED = 0x8000;
    }
}
