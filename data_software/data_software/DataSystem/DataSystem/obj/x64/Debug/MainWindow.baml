   M S B A M L   `   `   `  ����E  ADataSystem, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:DataSystem
DataSystem  5   	       DataSystem.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QScottPlot.WPF, Version=4.0.48.0, Culture=neutral, PublicKeyToken=e53b06131e34a3aaW SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:DataSystem     5   	   -   5          � Title$   ��5   	   $��20(�6   $��968.5��6    $
��1900��6/   .��5	      � $
��Auto��6   $	��886��6   $	��Top=�6%   $��0,10,8,0q�6=    J� RenderTransformOrigin$ 0.5,0.5&�6O    � ColumnDefinitions 5
   
   �� 5      $
��468* �6   �� 5      $
��257* �6   �� 5      $��1168* �6   .+�5   
   ]� $
��Left=�6   $	��876��6/   ��.  $��10,0,0,0q�6R   $	��Top=�6d   $
��1864��6|   $��3��6�   .+�5      � $	��919��6   $	��140��6    $��	0,0,0,-43q�6,    5      �� 5      $	��41* �6'   �� 5      $
��107* �6'   .+�5      o� -   5         o�Name$
 menu��5      $
��Left=�6%   $	��899��6@   $��Center=�6M   $	��181��6h   $��2��6t   $��10,10,-51,10q�6�   .@�5      m� -   5        $��net_2991��5   -   $��纹波采集��6   $��30(�6>   $��55��6e   m� 5      -   5        $��monitorControl��5   /   $��电机控制��6   $��30(�6F   $	��146��6T   6`   m� 5      -   5        $��make_data_collection��5   0   $��数据集制作��6   $��30(�6M   6�   m� 5      -   5        $��tHView��5   .   $��温度与电磁��6   $��30(�6=   $	��171��6K   $��55��6l   6z   m� 5      -   5        $��Chrome��5   -   $��电子负载��6   $��30(�6<   $	��143��6J   $��60��6k   6y   5      5      � 5      $��	0,0,0,-43q�6    � RowDefinitions 5      �� 5      $
��356* �6$   �� 5       $
��563* �6$   .+�5"      �� -   5         ��Name$ contentControl��5"   :   $��ContentControl��6!   $	��909��6R   $	��Top=�6_   $��2��6w   $��14(�6�   $��65,0,-1734,0q�6�   5#      5$      5%      5&      