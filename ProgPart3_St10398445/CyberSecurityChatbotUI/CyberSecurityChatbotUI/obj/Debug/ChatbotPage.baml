   M S B A M L   `   `   `  ����  CyberSecurityChatbotUI(    "CyberSecurityChatbotUI.ChatbotPage   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5        N� Title$  Cyber Security Chatbot��5       N� 
Background �	#FF0F0F23.��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   ���	#FF1E1E3F �� Padding$ 10q�65   ._�5      �� $��Cyber Security Chatbot��6   
���� ��$��24(�5      $
��Bold"�5      $��Center=�5      5      �� 5   
   $��1��6   $
��Auto=�6$   .��5      �� -   5         ��Name$ ChatDisplay��5       �� 
IsReadOnly .  �� TextWrapping$
 Wrap=�5       $��Consolas)�5!      
�������
������ $��0q�5$      " �� VerticalScrollBarVisibility$ Disabled=�5%      $��10q�5&      5'      � 5*   
   $��2��6   $��10q�6    � ColumnDefinitions 5+      �� 5,      $��* �6#   �� 5-      $
��Auto �6#   .+�50      �� -   5        $ 	UserInput��50      $��0��51      $��0,0,10,0q�52      
����������	#FF252550
���� ��56      �� 58      -   5        	 ��Name$	 
SendButton��58      $��1��59      $
��Send��5:      $
��15,5q�5;      
���� ��
�������5>      5?      �� 5B   
   -   5        $��3��5B      $��Back to Menu��5C      $��Center=�5D      $��10q�5E      $
��15,5q�5F      ���	#FF1E1E3F
�������5I      5J      5K      