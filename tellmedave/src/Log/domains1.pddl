(define (problem data_Nov-14-2014_1)
(:objects Robot PR2 Floor Wall_1 Wall_2 
Wall_3 Wall_4 Dormbed_1 Desk_1 Chair_1 
Almirah_1 Laptop_1 Speaker_1 Greenboard_1 Squarebedtable_1 
Stackbooks_1 Fireplace_1 Floorlamp_1 Blinds_1 Painting_1 
Painting_2 Blackcouch_1 Blackcouch_2 Squarebedtable_2 Humansit_1 
Humansit_2 Humansit_3 Garbagebin_1 Sink_1 Cup_1 
Bowl_1 Bowl_2 In On)
(:init 
(IsPlaceableOn Desk_1)
(IsPlaceableOn Chair_1)
(IsPlaceableOn Almirah_1)
(IsPlaceableOn Squarebedtable_1)
(IsGraspable Floorlamp_1)
(IsPlaceableOn Blackcouch_1)
(IsPlaceableOn Blackcouch_2)
(IsPlaceableOn Squarebedtable_2)
(IsPlaceableOn Sink_1)
(IsGraspable Bowl_1)
(IsGraspable Bowl_2)
(On Floorlamp_1 Squarebedtable_2)(On Bowl_1 Squarebedtable_2)(On Bowl_2 Blackcouch_2))(:goal (and (On Bowl_2 Chair_1)(On Bowl_1 Chair_1))))
