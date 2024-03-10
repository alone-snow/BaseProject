using UnityEngine;
public partial class LoginPanel : BasePanel
{
	public UnityEngine.UI.Button btnQuit;
	public UnityEngine.UI.Button btnBack;
	public UnityEngine.UI.Button btnRegister;
	public UnityEngine.UI.Button btnToRegister;
	public UnityEngine.UI.Button btnLogin;
	public TMPro.TMP_InputField ifRegisterPassword;
	public TMPro.TMP_InputField ifRegisterAccount;
	public TMPro.TMP_InputField ifLoginPassword;
	public TMPro.TMP_InputField ifLoginAccount;
	public UnityEngine.GameObject imgRegister;
	public UnityEngine.GameObject imgLogin;

	public override void Init(PanelUI ui)
	{
		btnQuit = ui.GetReference<UnityEngine.UI.Button>("btnQuit");
		btnBack = ui.GetReference<UnityEngine.UI.Button>("btnBack");
		btnRegister = ui.GetReference<UnityEngine.UI.Button>("btnRegister");
		btnToRegister = ui.GetReference<UnityEngine.UI.Button>("btnToRegister");
		btnLogin = ui.GetReference<UnityEngine.UI.Button>("btnLogin");
		ifRegisterPassword = ui.GetReference<TMPro.TMP_InputField>("ifRegisterPassword");
		ifRegisterAccount = ui.GetReference<TMPro.TMP_InputField>("ifRegisterAccount");
		ifLoginPassword = ui.GetReference<TMPro.TMP_InputField>("ifLoginPassword");
		ifLoginAccount = ui.GetReference<TMPro.TMP_InputField>("ifLoginAccount");
		imgRegister = ui.GetReference<UnityEngine.GameObject>("imgRegister");
		imgLogin = ui.GetReference<UnityEngine.GameObject>("imgLogin");
		string  btnQuitButtonName = "btnQuit";
		btnQuit.onClick.AddListener(()=>{ OnClick(btnQuitButtonName); });
		string  btnBackButtonName = "btnBack";
		btnBack.onClick.AddListener(()=>{ OnClick(btnBackButtonName); });
		string  btnRegisterButtonName = "btnRegister";
		btnRegister.onClick.AddListener(()=>{ OnClick(btnRegisterButtonName); });
		string  btnToRegisterButtonName = "btnToRegister";
		btnToRegister.onClick.AddListener(()=>{ OnClick(btnToRegisterButtonName); });
		string  btnLoginButtonName = "btnLogin";
		btnLogin.onClick.AddListener(()=>{ OnClick(btnLoginButtonName); });
		string  ifRegisterPasswordInputFieldName = "ifRegisterPassword";
		ifRegisterPassword.onValueChanged.AddListener((o)=>{ OnInputFieldValueChanged(ifRegisterPasswordInputFieldName, o); });
		string  ifRegisterAccountInputFieldName = "ifRegisterAccount";
		ifRegisterAccount.onValueChanged.AddListener((o)=>{ OnInputFieldValueChanged(ifRegisterAccountInputFieldName, o); });
		string  ifLoginPasswordInputFieldName = "ifLoginPassword";
		ifLoginPassword.onValueChanged.AddListener((o)=>{ OnInputFieldValueChanged(ifLoginPasswordInputFieldName, o); });
		string  ifLoginAccountInputFieldName = "ifLoginAccount";
		ifLoginAccount.onValueChanged.AddListener((o)=>{ OnInputFieldValueChanged(ifLoginAccountInputFieldName, o); });
	}

    protected override void OnClick(string btnName)
    {
        base.OnClick(btnName);
		switch (btnName)
		{
			case "btnRegister":
				Debug.Log("quit");
				break;
		}
    }
}
