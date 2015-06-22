(define (problem data_Nov-14-2014_1)
(:objects Robot PR2 Floor Wall_1 Wall_2 
Wall_3 Wall_4 Bed_1 Tv_1 Blackcouch_1 
Blackcouch_2 Roundtable_1 Almirah_1 Bedtable_1 Vase_1 
Wallshelf_1 Squaremirror_1 Humansit_1 Humanreach_1 Humansit_2 
Humansit_3 PR2 Floor Wall_1 Wall_2 
Wall_3 Wall_4 Shelf_1 Bed_1 Books_1 
Loungechair_1 Painting_1 Bedtable_1 Tablelamp_1 Floorlamp_1 
Loungechair_2 Almirah_1 Blackcouch_1 Wallshelf_1 Vase_1 
Vase_2 Antiquetable_1 Squaremirror_1 Humansit_1 Humansit_2 
Humansit_3 Humanstand_1 PR2 Floor Wall-1 
Wall-2 Wall-3 Wall-4 Bed-0.0 Windows-0.0 
Desk-0.0 Bed-1.0 Chair-0.0 Bedtable-0.0 Bedtable-1.0 
Tablelamp-0.0 Desk-1.0 Chair-1.0 Books-0.0 Floorlamp-0.0 
Laptop-1.0 Bookshelf-0.0 Walllamp-0.0 Humanreach-0.0 Humansit-0.0 
Humanlean-0.0 PR2 Floor Wall-1 Wall-2 
Wall-3 Wall-4 Roundbed-0.0 Antiquetable-0.0 Squarebedtable-0.0 
Squarebedtable-1.0 Tablelamp-0.0 Tablelamp-1.0 Painting-0.0 Painting-1.0 
Painting-2.0 Floorlamp-0.0 Walllamp-0.0 Chandelier-0.0 Ornatecurtains-0.0 
Almirah-0.0 Antiquetable-1.0 Vase-0.0 Humanlean-0.0 Humansit-0.0 
Humanstand-0.0 Humanlean-1.0 PR2 Floor Wall_1 
Wall_2 Wall_3 Wall_4 Bed_1 Dressingmirror_1 
Squarebedtable_1 Tablelamp_1 Painting_1 Painting_2 Floorlamp_1 
Window_1 Window_2 Stackbooks_1 Vase_1 Candleholder_1 
Walllamp_1 Antiquetable_1 Flowervase_1 Fireplace_1 Humansit_1 
Humanstand_1 Humanlean_2 Humansit_2 PR2 Floor 
Wall-1 Wall-2 Wall-3 Wall-4 Bunkbed-0.0 
Wallshelf-0.0 Bookshelf-0.0 Tablelamp-0.0 Desk-0.0 Tablechair-0.0 
Tablechair-1.0 Stackbooks-0.0 Painting-0.0 Painting-1.0 Painting-2.0 
Window-0.0 Floorlamp-0.0 Squarebedtable-0.0 Tablelamp-1.0 Walllamp-0.0 
Almirah-1.0 Blackcouch-0.0 Antiquetable-0.0 Squaremirror-0.0 Humansit-0.0 
Humansit-1.0 Humanstand-0.0 PR2 Floor Wall_1 
Wall_2 Wall_3 Wall_4 Dormbed_1 Desk_1 
Chair_1 Almirah-0.0 Laptop-0.0 Speaker-0.0 Greenboard-0.0 
Squarebedtable_1 Stackbooks-0.0 Fireplace-0.0 Floorlamp_1 Blinds-0.0 
Painting-0.0 Painting-1.0 Blackcouch_1 Blackcouch-1.0 Squaredbedtable_2 
Humansit_1 Humansit_2 Humansit_3 PR2 Floor 
Wall-1 Wall-2 Wall-3 Wall-4 Bed-0.0 
Windows-0.0 Desk-0.0 Bed-1.0 Chair-0.0 Bedtable-0.0 
Bedtable-1.0 Tablelamp-0.0 Desk-1.0 Chair-1.0 Books-0.0 
Floorlamp-0.0 Laptop-1.0 Almirah-0.0 Walllamp-0.0 Humanlean-0.0 
Humansit-0.0 PR2 Floor Wall_1 Wall_2 
Wall_3 Wall_4 Speaker-0.0 Speaker-1.0 Projectionarea-0.0 
Tallspeaker-1.0 Tallspeaker-2.0 Tallspeaker-3.0 Tallspeaker-4.0 Tallspeaker-5.0 
Sofa_1 Loungechair_2 Roundtable_1 Floorlamp_1 Sidetable_1 
Vase_1 Walllamp-0.0 Walllamp-1.0 Humansit_1 Humansit_2 
In On)
(:init 
(Pressable Tv_1)
(IsPlaceableOn Blackcouch_1)
(IsPlaceableOn Blackcouch_2)
(IsPlaceableOn Shelf_1)
(IsPlaceableOn Blackcouch_1)
(IsPlaceableOn Blackcouch_1)
(Near Robot Sidetable_1))(:goal (and (state Tv_1 IsOn))))
