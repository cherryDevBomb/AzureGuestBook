<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="GuestBookWebRole._Default" %>

<!DOCTYPE html>

<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Guestbook</title>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css" integrity="sha384-Gn5384xqQ1aoWXA+058RXPxPg6fy4IWvTNh0E263XmFcJlSAwiGgFAW/dAiS6JXm" crossorigin="anonymous">
    <link rel="stylesheet" type="text/css" href="Content/bootstrap.css">
    <link rel="stylesheet" type="text/css" href="Content/Site.css">
</head>

<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1"
            runat="server">
        </asp:ScriptManager>
        <asp:Timer runat="server" id="Timer1" interval="8000" ontick="Timer1_Tick" Enabled="false" />

        <nav class="navbar navbar-light bg-dark rounded-0 pt-1">
            <h2 class="text-light">GuestBook</h2>
        </nav>

        <div class="jumbotron">

            <div>
                <div class="form-group row">
                    <div class="col-2 col-md-2 col-lg-1 pr-0">
                        <label for="NameLabel">Name:</label>
                    </div>
                    <div class="col-10 col-md-5 col-lg-5 pl-0">
                        <asp:TextBox
                            ID="NameTextBox"
                            runat="server"
                            class="form-control" />
                        <asp:RequiredFieldValidator
                            ID="NameRequiredValidator"
                            runat="server"
                            ControlToValidate="NameTextBox"
                            Text=".*" />
                    </div>
                </div>

                <div class="form-group row">
                    <div class="col-2 col-md-2 col-lg-1 pr-0">
                        <label for="MessageLabel">Message:</label>
                    </div>
                    <div class="col-10 col-md-5 col-lg-5 pl-0">
                        <asp:TextBox
                            ID="MessageTextBox"
                            runat="server"
                            class="form-control"
                            TextMode="MultiLine"
                            Rows="5"
                            ToolTip="Enter comment here" />
                    </div>
                </div>

                <div class="form-group row">
                    <div class="col-2 col-md-2 col-lg-1 pr-0">
                        <label for="MessageLabel">Picture:</label>
                    </div>
                    <div class="col-10 col-md-5 col-lg-5 pl-0">
                        <asp:FileUpload
                            ID="FileUpload1"
                            runat="server"></asp:FileUpload>
                    </div>
                </div>

                <div class="row">
                    <div class="col-12 col-md-7 col-lg-6 text-right">
                        <asp:Button ID="SignButton"
                            Text="Submit"
                            OnClick="SignButton_Click"
                            runat="server"
                            class="btn btn-info"></asp:Button>
                    </div>
                </div>

            </div>

            <div class="py-5 mt-3">
                <h3>Comments</h3>

                <asp:DataList ID="DataList1"
                    BorderColor="black"
                    CellPadding="5"
                    CellSpacing="5"
                    RepeatDirection="Vertical"
                    RepeatLayout="Flow"
                    RepeatColumns="0"
                    BorderWidth="0"
                    runat="server"
                    style="margin-top:1em;">

                    <ItemTemplate>
                        <div class="card mw-100">
                            <div class="row">
                                <div class="col-auto">
                                    <asp:Image ID="Image" class="card-img-left" ImageUrl='<%# DataBinder.Eval(Container.DataItem, "ThumbnailUrl") %>' runat="server" />
                                </div>
                                <div class="col">
                                    <div class="card-block px-2 py-3">
                                        <h4 class="card-title">
                                            <strong>
                                                <%# DataBinder.Eval(Container.DataItem,"GuestName") %>
                                            </strong>
                                        </h4>
                                        <p class="card-text">
                                            <%# DataBinder.Eval(Container.DataItem,"Message") %>
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>

                </asp:DataList>
            </div>
        </div>
    </form>
</body>
</html>
