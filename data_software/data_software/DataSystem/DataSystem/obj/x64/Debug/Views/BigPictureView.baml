   M S B A M L   `   `   `  ����E  ADataSystem, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:DataSystem.ViewsDataSystem.Views  5      %    DataSystem.Views.BigPictureView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QScottPlot.WPF, Version=4.0.48.0, Culture=neutral, PublicKeyToken=e53b06131e34a3aaW SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      0localclr-namespace:DataSystem.Views     5      .��5	      � 
�������  � RowDefinitions  5
   
   �� 5      $	��29* �6   �� 5       � ColumnDefinitions 5   
   �� 5      $	��59* �6   �� 5      $	��41* �6   .+�5   
     ScottPlot.WpfPlot  -   5        $��	plotScott��5      $
��Left=�6#   $��	10,10,0,0q�6>   $	��Top=�6Q   $	��632��6i   $
��1630��6v   $��2��6�   �� 5   
   -   5         ��Name$ 
start_stop��5      $��1��6%   $��暂停��65   $��25(�6B   $
��Left=�6P   $��519,709,0,0q�6k   $	��Top=�6�   $��54��6�   $	��148��6�   6�   �� 5   
   -   5         ��Name$ label��5      $��Label��6   $
��Left=�6/   $��
94,647,0,0q�6J   $	��Top=�6^   $
��1009��6v   $	��116��6�   $��2��6�   5      5      