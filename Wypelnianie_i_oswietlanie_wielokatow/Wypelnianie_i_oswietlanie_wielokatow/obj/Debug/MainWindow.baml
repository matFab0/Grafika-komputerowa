   M S B A M L   `   `   `  ����_  [Wypelnianie_i_oswietlanie_wielokatow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null[2clr-namespace:Wypelnianie_i_oswietlanie_wielokatow$Wypelnianie_i_oswietlanie_wielokatow  5   	   5    /Wypelnianie_i_oswietlanie_wielokatow.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Dlocal2clr-namespace:Wypelnianie_i_oswietlanie_wielokatow     5   	   -   5          � Title$  
MainWindow��5   	   $	��900��6   $
��1600��6)    � WindowStartupLocation$ CenterScreen=�66   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $	��80* �6   �� 5      $	��20* �6   .+�5   
   �� -   5         ��Name$ canvas��5      
�������.+�5      �� -   5         ��Name$ myImage��5      5      � 5   
   $��1��6   
������� � RowDefinitions 5      �� 5      $	��10* �6    �� 5      $	��20* �6    �� 5      $	��20* �6    �� 5      $	��40* �6    �� 5      $	��20* �6    .+�5      �� $��	Rysowanie��6   $��0��6*   .��5      �� -   5         ��Name$ polygonButton��5      $��Rysuj wielokąt��60   $��2q�6e   5      �� 5      $��Edytuj��6   $��1��6'   .��5      �� .+�5      �� -   5        $ pointButton��5      $��Przesuń punkt��62   $��2q�6d   �� 5      -   5        $ polygonButton2��5      $��Przesuń wielokąt��65   $��2q�6n   �� 5       -   5        $ deletePointButton��5       $��Usuń punkt��68   $��2q�6m   �� 5!      -   5        $ addPointButton��5!      $��Dodaj punkt��65   $��2q�6h   �� 5"      -	   5        $ changeLightPositionButton��5"      $ ��Zmień pozycję światłą��6@   $��2q�6�   5#      5$      �� 5%      $��Kolory��6   $��2��6'   .��5&      �� .+�5'      �� -
   5        $ pointColorButton��5'      $��Zmień kolor wierzchołka��66   $��2q�6w   �� 5(      -   5        $ polygonColorButton��5(      $��Zmień kolor wielokąta��69   $��2q�6z   �� 5)      -   5        $  polygonInterpolationButton��5)      $��Interpolacja wielokąta��6A   $��2q�6�   �� 5*      -   5        $ polygonBitmapButton��5*      $#��Wczytaj bitmapę na wielokąt��6:   $��2q�6�   �� 5+      -   5        $ heightMapButton��5+      $!��Wczytaj bitmapę wysokości��66   $��2q�6x   5,      5-      �� 5/      $��Suwaczki��6   $��3��6)   .��50      �� .+�51      ]� $��2q�6    .+�52      �� $��kd��6    $
��Left=�6-   
�������6C   �� 53      $��Right=�6y    �� TextAlignment$ Center=�6�   $��40��6�   ��6"   ��  �� ElementName$ kdSlider��	 �� Path$	 Value�
 �� UpdateSourceTrigger$
 PropertyChanged=�6�   �� 54      -   5         ��Name$ kdSlider��54   !    � Maximum$ 1Q�63    � Minimum$ 0Q�6?    � Value$	 0.5Q�6K    �� TickPlacement$ BottomRight=�6W    �� TickFrequency$
 0.01Q�6s    �� IsSnapToTickEnabled . 6�   55      ]� 56      $��2q�6    .+�57      �� $��ks��6    $
��Left=�6-   
�������6C   �� 58      $��Right=�6y   $ Center=�6�   $��40��6�   ��6"   �� $ ksSlider��$	 Value�$
 PropertyChanged=�6�   �� 59      -   5        $ ksSlider��59   !   $ 1Q�63   $ 0Q�6?   $	 0.5Q�6K   $ BottomRight=�6W   $
 0.01Q�6s    . 6�   5:      ]� 5;      $��2q�6    .+�5<      �� $��m��6    $
��Left=�6,   
�������6B   �� 5=      $��Right=�6x   $ Center=�6�   $��40��6�   ��6"   �� $ mSlider��$	 Value�$
 PropertyChanged=�6�   �� 5>      -   5        $ mSlider��5>   !   $	 100Q�62   $ 1Q�6@   $ BottomRight=�6L   $ 1Q�6h    . 6�   5?      ]� 5@      $��2q�6    .+�5A      �� $��z��6    $
��Left=�6,   
�������6B   �� 5B      $��Right=�6x   $ Center=�6�   $��40��6�   ��6"   �� $ zSlider��$	 Value�$
 PropertyChanged=�6�   �� 5C      -   5        $ zSlider��5C   !   $
 1000Q�62   $	 0.1Q�6A   $ BottomRight=�6O   $	 0.1Q�6k    . 6�   5D      ]� 5E      $��2q�6    .+�5F      �� $��Light R��6    $
��Left=�62   
�������6H   �� 5G      $��Right=�6}   $ Center=�6�   $��40��6�   ��6"   �� $ lightRSlider��$	 Value�$
 PropertyChanged=�6�   �� 5H      -   5        $ lightRSlider��5H   !   $ 1Q�67   $ 0Q�6C   $ 1Q�6O   $ BottomRight=�6Y   $
 0.01Q�6u    . 6�   5I      ]� 5J      $��2q�6    .+�5K      �� $��Light G��6    $
��Left=�62   
�������6H   �� 5L      $��Right=�6}   $ Center=�6�   $��40��6�   ��6"   �� $ lightGSlider��$	 Value�$
 PropertyChanged=�6�   �� 5M      -   5        $ lightGSlider��5M   !   $ 1Q�67   $ 0Q�6C   $ 1Q�6O   $ BottomRight=�6Y   $
 0.01Q�6u    . 6�   5N      ]� 5O      $��2q�6    .+�5P      �� $��Light B��6    $
��Left=�62   
�������6H   �� 5Q      $��Right=�6}   $ Center=�6�   $��40��6�   ��6"   �� $ lightBSlider��$	 Value�$
 PropertyChanged=�6�   �� 5R      -   5        $ lightBSlider��5R   !   $ 1Q�67   $ 0Q�6C   $ 1Q�6O   $ BottomRight=�6Y   $
 0.01Q�6u    . 6�   5S      5T      5U      �� 5V      $��Scena��6   $��4��6&   .��5W      �� .+�5X      �� -   5        $ startButton��5X      $��Start��62   $��2q�6\   �� 5Y      -   5        $ 
stopButton��5Y      $
��Stop��61   $��2q�6Y   ]� 5Z      .+�5[      �� $��Zakres prędkośći��6    $��Center=�6;   $	��Top=�6X   
�������6m   �� 5\      -   5         ��Name$ minSpeedTextBox��5\   "   $
��Left=�6;   $	��145��6Q   6]   �� 5]      $��-��6    �� 5^      -   5        $ maxSpeedTextBox��5^   "   $��Right=�6;   6R   5_      5`      5a      5b      5c      5d      