abstract_equals([], [], Current, Result):-
	copy_term(Current, Result).

abstract_equals([H1|T1], [H2|T2], Current, Result):-
	\+ number(H1), \+ number(H2),
	H1 == H2, abstract_equals(T1, T2, Current, Result).


abstract_equals([H1|T1], [H2|T2], Current, Result):-
	member([H1, Pair], Current),
	Pair == H2, abstract_equals(T1, T2, Current, Result).
	
abstract_equals([H1|T1], [H2|T2], Current, Result):-
	number(H1), number(H2),
	not_member_first([H1, _], Current),
	not_member_second([_, H2], Current),
	append(Current, [[H1, H2]], NewCurrent),
	abstract_equals(T1, T2, NewCurrent, Result).


not_member_first([_, _], []).
not_member_first([Source, Renamed], [[Source1, _]|T]):-
	Source \= Source1,
	not_member_first([Source, Renamed], T).
	
not_member_second([_, _], []).
not_member_second([Source, Renamed], [[_, Renamed1]|T]):-
	Renamed \= Renamed1,
	not_member_second([Source, Renamed], T).


saturate_group(Group, Example, Obj, Result):-
	object(id(Obj), Example, in, Group, _, _, _, _, _, _, _),
	Result = [group, Obj].
	
saturate_number(Number, Example, Result):-
		object(id(Obj), Example, in, _, Number, _, _, _, _, _, _),
		Result = [x, Obj].
		
saturate_number(Number, Example, Result):-
		object(id(Obj), Example, in, _, _, Number, _, _, _, _, _),
		Result = [y, Obj].
		
saturate_number(Number, Example, Result):-
		object(id(Obj), Example, in, _, _, _, Number, _, _, _, _),
		Result = [width, Obj].
		
saturate_number(Number, Example, Result):-
		object(id(Obj), Example, in, _, _, _, _, Number, _, _, _),
		Result = [height, Obj].
		
saturate_number_comp(Number, Example, Result):-
	saturate_number(N1, Example, Result1),
	saturate_number(N2, Example, Result2),
	Number is N1 + N2,
	Result = [add, Result1, Result2].
	
saturate_number_comp(Number, Example, Result):-
	saturate_number(N1, Example, Result1),
	saturate_number(N2, Example, Result2),
	Number is N1 - N2,
	Result = [sub, Result1, Result2].
	
saturate_number_mul(Number, Example, Result):-
	saturate_number(N1, Example, Result1),
	saturate_number(N2, Example, Result2),
	Number is N1 * N2,
	Result = [mul, Result1, Result2].
	
saturate_number_div(Number, Example, Result):-
	saturate_number(N1, Example, Result1),
	saturate_number(N2, Example, Result2),
	Number is N1 / N2,
	Result = [add, Result1, Result2].

saturate_color(Color, Example, Obj, Region, Result):-
	object(id(Obj), Example, in, _, _, _, _, _, Colors, _, _),
	nth0(Region, Colors, Color),
	Result = [region, Region, Obj].

saturate_color_iter([], [], _, _, _, _).

saturate_color_iter([C1|T1], [C2|T2], EXID, RC, T0, T):-
	saturate_color(C1, EXID, _, _, RC1),
	saturate_color(C2, EXID, _, _, RC2),
	flatten(RC1, FC1), flatten(RC2, FC2),
	abstract_equals(FC1, FC2, T0, T),
	saturate_color_iter(T1, T2, EXID, RCrec, T, T),
	append(RCrec, [RC1], RC),!.
	
saturate(Id1, Id2, EXID, T, EXPR):-
	object(Id1, EXID, out, G1, X1, Y1, W1, H1, C1, _, _),
	object(Id2, EXID, out, G2, X2, Y2, W2, H2, C2, _, _),
	
	saturate_group(G1, EXID, _, RG1),
	saturate_group(G2, EXID, _, RG2),
	flatten(RG1, FG1), flatten(RG2, FG2),
	abstract_equals(FG1, FG2, [], T0),
	
	saturate_number(X1, EXID, RX1),
	saturate_number(X2, EXID, RX2),
	flatten(RX1, FX1), flatten(RX2, FX2),
	abstract_equals(FX1, FX2, T0, T1),
	
	saturate_number(Y1, EXID, RY1),
	saturate_number(Y2, EXID, RY2),
	flatten(RY1, FY1), flatten(RY2, FY2),
	abstract_equals(FY1, FY2, T1, T2),
	
	saturate_number(W1, EXID, RW1),
	saturate_number(W2, EXID, RW2),
	flatten(RW1, FW1), flatten(RW2, FW2),
	abstract_equals(FW1, FW2, T2, T3),
	
	saturate_number(H1, EXID, RH1),
	saturate_number(H2, EXID, RH2),
	flatten(RH1, FH1), flatten(RH2, FH2),
	abstract_equals(FH1, FH2, T3, T4),
	
	saturate_color_iter(C1, C2, EXID, RC1, T4, T),
	
	EXPR = [RG1, RX1, RY1, RW1, RH1, RC1].

object(id(0), ex_id(0), in, group(2), 0, 0, 7, 7, [5, 2], null, null).
object(id(2), ex_id(0), in, group(0), 1, 13, 3, 3, [1], null, null).
object(id(3), ex_id(0), in, group(3), 1, 13, 3, 3, [1], null, null).
object(id(6), ex_id(0), in, group(4), 2, 2, 3, 3, [2], null, id(0)).
object(id(8), ex_id(0), in, group(4), 7, 10, 3, 3, [1], null, null).
object(id(11), ex_id(0), in, group(0), 12, 3, 4, 1, [1], null, null).
object(id(12), ex_id(0), in, group(6), 12, 3, 4, 1, [1], null, null).
object(id(15), ex_id(0), in, group(4), 15, 13, 3, 3, [1], null, null).

object(id(0), ex_id(0), out, group(2), 0, 0, 7, 7, [5, 2], null, null).
object(id(2), ex_id(0), out, group(0), 1, 13, 3, 3, [1], null, null).
object(id(3), ex_id(0), out, group(3), 1, 13, 3, 3, [1], null, null).
object(id(6), ex_id(0), out, group(4), 2, 2, 3, 3, [2], null, id(0)).
object(id(8), ex_id(0), out, group(4), 7, 10, 3, 3, [1], null, null).
object(id(11), ex_id(0), out, group(0), 12, 3, 4, 1, [1], null, null).
object(id(12), ex_id(0), out, group(6), 12, 3, 4, 1, [1], null, null).
object(id(15), ex_id(0), out, group(4), 15, 13, 3, 3, [1], null, null).

object(id(61), ex_id(1), in, group(18), 1, 5, 7, 7, [5, 3], null, null).
object(id(63), ex_id(1), in, group(0), 3, 7, 3, 3, [3], null, id(61)).
object(id(64), ex_id(1), in, group(19), 3, 7, 3, 3, [3], null, id(61)).
object(id(67), ex_id(1), in, group(0), 9, 15, 3, 3, [1], null, null).
object(id(68), ex_id(1), in, group(19), 9, 15, 3, 3, [1], null, null).
object(id(72), ex_id(1), in, group(0), 11, 11, 1, 3, [1], null, null).
object(id(73), ex_id(1), in, group(21), 11, 11, 1, 3, [1], null, null).
object(id(76), ex_id(1), in, group(0), 12, 3, 3, 3, [1], null, null).
object(id(77), ex_id(1), in, group(19), 12, 3, 3, 3, [1], null, null).
object(id(81), ex_id(1), in, group(0), 16, 10, 5, 3, [1], null, null).
object(id(82), ex_id(1), in, group(23), 16, 10, 5, 3, [1], null, null).
object(id(61), ex_id(1), out, group(18), 1, 5, 7, 7, [5, 3], null, null).
object(id(63), ex_id(1), out, group(0), 3, 7, 3, 3, [3], null, id(61)).
object(id(64), ex_id(1), out, group(19), 3, 7, 3, 3, [3], null, id(61)).
object(id(67), ex_id(1), out, group(0), 9, 15, 3, 3, [1], null, null).
object(id(68), ex_id(1), out, group(19), 9, 15, 3, 3, [1], null, null).
object(id(72), ex_id(1), out, group(0), 11, 11, 1, 3, [1], null, null).
object(id(73), ex_id(1), out, group(21), 11, 11, 1, 3, [1], null, null).
object(id(76), ex_id(1), out, group(0), 12, 3, 3, 3, [1], null, null).
object(id(77), ex_id(1), out, group(19), 12, 3, 3, 3, [1], null, null).
object(id(81), ex_id(1), out, group(0), 16, 10, 5, 3, [1], null, null).
object(id(82), ex_id(1), out, group(23), 16, 10, 5, 3, [1], null, null).
object(id(123), ex_id(2), in, group(0), 0, 0, 6, 6, [0], null, null).
object(id(124), ex_id(2), in, group(34), 0, 0, 6, 6, [5, 2], null, null).
object(id(134), ex_id(2), in, group(37), 0, 13, 7, 7, [5, 2], null, null).
object(id(136), ex_id(2), in, group(0), 1, 2, 1, 3, [2], null, id(124)).
object(id(137), ex_id(2), in, group(36), 1, 2, 1, 3, [2], null, id(124)).
object(id(141), ex_id(2), in, group(0), 2, 15, 3, 2, [2], null, id(134)).
object(id(142), ex_id(2), in, group(39), 2, 15, 3, 2, [2], null, id(134)).
object(id(145), ex_id(2), in, group(0), 10, 4, 1, 3, [1], null, null).
object(id(146), ex_id(2), in, group(36), 10, 4, 1, 3, [1], null, null).
object(id(150), ex_id(2), in, group(0), 11, 10, 3, 2, [1], null, null).
object(id(151), ex_id(2), in, group(39), 11, 10, 3, 2, [1], null, null).
object(id(155), ex_id(2), in, group(42), 15, 7, 3, 3, [1], null, null).
object(id(157), ex_id(2), in, group(0), 16, 2, 3, 2, [1], null, null).
object(id(158), ex_id(2), in, group(39), 16, 2, 3, 2, [1], null, null).
object(id(123), ex_id(2), out, group(0), 0, 0, 6, 6, [0], null, null).
object(id(124), ex_id(2), out, group(34), 0, 0, 6, 6, [5, 2], null, null).
object(id(134), ex_id(2), out, group(37), 0, 13, 7, 7, [5, 2], null, null).
object(id(136), ex_id(2), out, group(0), 1, 2, 1, 3, [2], null, id(124)).
object(id(137), ex_id(2), out, group(36), 1, 2, 1, 3, [2], null, id(124)).
object(id(141), ex_id(2), out, group(0), 2, 15, 3, 2, [2], null, id(134)).
object(id(142), ex_id(2), out, group(39), 2, 15, 3, 2, [2], null, id(134)).
object(id(145), ex_id(2), out, group(0), 10, 4, 1, 3, [1], null, null).
object(id(146), ex_id(2), out, group(36), 10, 4, 1, 3, [1], null, null).
object(id(150), ex_id(2), out, group(0), 11, 10, 3, 2, [1], null, null).
object(id(151), ex_id(2), out, group(39), 11, 10, 3, 2, [1], null, null).
object(id(155), ex_id(2), out, group(42), 15, 7, 3, 3, [1], null, null).
object(id(157), ex_id(2), out, group(0), 16, 2, 3, 2, [1], null, null).
object(id(158), ex_id(2), out, group(39), 16, 2, 3, 2, [1], null, null).
object(id(206), ex_id(3), in, group(54), 0, 13, 7, 6, [5, 3], null, null).
object(id(208), ex_id(3), in, group(0), 1, 15, 4, 3, [3], null, id(206)).
object(id(209), ex_id(3), in, group(55), 1, 15, 4, 3, [3], null, id(206)).
object(id(217), ex_id(3), in, group(58), 3, 1, 7, 7, [5, 3], null, null).
object(id(219), ex_id(3), in, group(0), 5, 3, 3, 3, [3], null, id(217)).
object(id(220), ex_id(3), in, group(59), 5, 3, 3, 3, [3], null, id(217)).
object(id(223), ex_id(3), in, group(0), 8, 15, 4, 3, [1], null, null).
object(id(224), ex_id(3), in, group(55), 8, 15, 4, 3, [1], null, null).
object(id(234), ex_id(3), in, group(0), 9, 10, 3, 3, [1], null, null).
object(id(235), ex_id(3), in, group(59), 9, 10, 3, 3, [1], null, null).
object(id(239), ex_id(3), in, group(0), 12, 5, 1, 4, [1], null, null).
object(id(240), ex_id(3), in, group(64), 12, 5, 1, 4, [1], null, null).
object(id(243), ex_id(3), in, group(65), 15, 10, 3, 3, [1], null, null).
object(id(206), ex_id(3), out, group(54), 0, 13, 7, 6, [5, 3], null, null).
object(id(208), ex_id(3), out, group(0), 1, 15, 4, 3, [3], null, id(206)).
object(id(209), ex_id(3), out, group(55), 1, 15, 4, 3, [3], null, id(206)).
object(id(217), ex_id(3), out, group(58), 3, 1, 7, 7, [5, 3], null, null).
object(id(219), ex_id(3), out, group(0), 5, 3, 3, 3, [3], null, id(217)).
object(id(220), ex_id(3), out, group(59), 5, 3, 3, 3, [3], null, id(217)).
object(id(223), ex_id(3), out, group(0), 8, 15, 4, 3, [1], null, null).
object(id(224), ex_id(3), out, group(55), 8, 15, 4, 3, [1], null, null).
object(id(234), ex_id(3), out, group(0), 9, 10, 3, 3, [1], null, null).
object(id(235), ex_id(3), out, group(59), 9, 10, 3, 3, [1], null, null).
object(id(239), ex_id(3), out, group(0), 12, 5, 1, 4, [1], null, null).
object(id(240), ex_id(3), out, group(64), 12, 5, 1, 4, [1], null, null).
object(id(243), ex_id(3), out, group(65), 15, 10, 3, 3, [1], null, null).