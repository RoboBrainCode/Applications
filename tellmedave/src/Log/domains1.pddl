(define (problem data_Nov-14-2014_1)
(:objects Robot PR2 Floor Wall_1 Wall_2 
Wall_3 Wall_4 Bed_1 Tv_1 Blackcouch_1 
Blackcouch_2 Roundtable_1 Almirah_1 Bedtable_1 Vase_1 
Wallshelf_1 Squaremirror_1 Humansit_1 Humanreach_1 Humansit_2 
In On)
(:init 
(IsPlaceableOn Bed_1)
(Pressable Tv_1)
(IsPlaceableOn Blackcouch_1)
(IsPlaceableOn Blackcouch_2)
(IsPlaceableOn Roundtable_1)
(IsPlaceableOn Almirah_1)
(IsPlaceableOn Bedtable_1)
(IsGraspable Vase_1)
(IsPlaceableOn Wallshelf_1)
(On Vase_1 Bedtable_1)(Near Robot Bed_1))(:goal (and (state Tv_1 Channel1)(state Tv_1 IsOn))))
