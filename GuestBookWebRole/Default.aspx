<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="GuestBookWebRole._Default" %>

<!DOCTYPE html>

<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Guestbook</title>
</head>

<body>
    <form id="form1" runat="server">
           <asp:ScriptManager ID="ScriptManager1" 
               runat="server">
           </asp:ScriptManager>
            <asp:Timer runat="server" id="Timer1" interval="500000" ontick="Timer1_Tick" />
            <div class="general">
                <div class="title">
                    <h1>
                        GuestBook
                    </h1>
                </div>
                <div class="inputSection">
                    <dl>
                        <dt>
                            <label for="NameLabel">Name:</label>
                        </dt>
                        <dd>
                            <asp:TextBox
                                ID="NameTextBox"
                                runat="server"
                                 />
                            <asp:RequiredFieldValidator
                                ID="NameRequiredValidator"
                                runat="server"
                                ControlToValidate="NameTextBox"
                                Text="*" />
                        </dd>
                        <dt>
                            <label for="MessageLabel">Message:</label>
                        </dt>
                        <dd>
                            <asp:TextBox
                                ID="MessageTextBox"
                                runat="server"
                                 TextMode="MultiLine"
                                 BorderStyle="Inset"
                                  Rows="5"
                                 Columns="45"
                               CssClass="MultiLineTextBox"
                                ToolTip="Enter comments here"/>
                  
                        </dd>
                          <dt>
                            <label for="MessageLabel">Picture:</label>
                        </dt>
                        <dd>
                             <asp:FileUpload 
                               ID="FileUpload1"
                               runat="server">
                            </asp:FileUpload>
       
                            <br /><br />
       
                            <asp:Button id="SignButton" 
                                Text="Write comment"
                                OnClick="SignButton_Click"
                                runat="server">
                            </asp:Button>

                        </dd>
                    </dl>
                </div>
            </div>
            <div>

               <h3>GuestBook Comments</h3>
 
      <asp:DataList id="DataList1"
           BorderColor="black"
           CellPadding="5"
           CellSpacing="5"
           RepeatDirection="Vertical"
           RepeatLayout="Table"
           RepeatColumns="0"
           BorderWidth="0"
           runat="server">

         <HeaderStyle BackColor="#aaaadd">
         </HeaderStyle>

         <AlternatingItemStyle BackColor="Gainsboro">
         </AlternatingItemStyle>

         <HeaderTemplate>

            List of comments

         </HeaderTemplate>
               
         <ItemTemplate>

             Guest Name: <%# DataBinder.Eval(Container.DataItem,"GuestName") %>
             <br></br>

            Message: <%# DataBinder.Eval(Container.DataItem,"Message") %>

             <br></br>

             <asp:Image ID="Image" ImageUrl='<%# DataBinder.Eval(Container.DataItem, "ThumbnailUrl") %>' runat="server" />

         </ItemTemplate>

      </asp:DataList>
          </div>
        </form>
</body>
</html>
