   M S B A M L   `   `   `  ����I  EOEM_Serial_WPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:OEM_Serial_WPFOEM_Serial_WPF  5   	       OEM_Serial_WPF.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   .localclr-namespace:OEM_Serial_WPF     5   	     � Title$  OEM Serial Example��5   	   $	��350��6$   $	��756��61   .��5	      � .+�5
   
   �� -   5         ��Name$ button_Connect��5
      $��Connect��6)   $
��Left=�6;   $��	10,20,0,0q�6V   $	��Top=�6i   $	��100��6�   $��30��6�   {�.  6�   �� 5   
   -   5        $ button_Refresh��5      $��Refresh��6)   $
��Left=�6;   $��
115,20,0,0q�6V   $	��Top=�6j   $	��100��6�   $��30��6�   6�   �� 5   
   -   5         ��Name$ comboBox��5      $
��Left=�6%   $��	10,55,0,0q�6@   $	��Top=�6S   $	��205��6k   $��30��6w   6�   �� 5   
   $
��Left=�6   $��7��6/   $��	10,90,0,0q�6:   $	��Top=�6M   $	��205��6e   �� 5   
   -   5         ��Name$ groupBox_IntegrationTime��5      $��Integration Time��65   $
��Left=�6O   $��
10,102,0,0q�6j   $	��Top=�6~   $��60��6�   $	��205��6�   .��5      � $
��Left=�6   $��0,0,-2,0q�6.   $	��Top=�6@   $	��195��6X   $��40��6d   .+�5      �� -   5        $ button_getIntegrationTime��5      $	��Get��6<   $
��Left=�6J   $��	10,10,0,0q�6e   $	��Top=�6x   $��40��6�   $��20��6�   6�   �� 5      -   5        $ button_setIntegrationTime��5      $	��Set��6<   $
��Left=�6J   $��	55,10,0,0q�6e   $	��Top=�6x   $��40��6�   $��20��6�   6�   �� 5      -   5         ��Name$ textBox_integrationTime��5      $
��Left=�6;   $��20��6V   $��
100,10,0,0q�6b    �� TextWrapping$
 Wrap=�6v   $��0��6�   $	��Top=�6�   $��85��6�   6�   5      5      �� 5   
   -   5        $ button_Ping��5      $��Ping Device��6&   $
��Left=�6<   $��
10,167,0,0q�6W   $	��Top=�6k   $	��100��6�   $��30��6�   6�   �� 5   
   -	   5        $ button_Acquire��5      $��Acq. Spectrum��6)   $
��Left=�6A   $��115,167,0,0q�6\   $	��Top=�6q   $	��100��6�   $��30��6�   6�   �� 5   
   -
   5        $ button_FirmwareRev��5      $��Firmware Rev��6-   $
��Left=�6D   $��
10,202,0,0q�6_   $	��Top=�6s   $	��100��6�   $��30��6�   6�   �� 5   
   -   5        $ button_FpgaRev��5      $��FPGA Rev��6)   $
��Left=�6<   $��115,202,0,0q�6W   $	��Top=�6l   $	��100��6�   $��30��6�   6�   �� 5   
   -   5         ��Name$ label_Output��5      $��Output��6&   $
��Left=�67   $��
220,20,0,0q�6R   $	��Top=�6f   $	��387��6~   �� 5   
   -   5        $ textBox_Output��5      $
��Left=�5      $	��258��5      $	��518��6   $��
220,51,0,0q�5      $
 Wrap=�5       $�� ��5!      $	��Top=�5"       �� 
IsReadOnly . $��Visible=�5$      6@   5&      5'      