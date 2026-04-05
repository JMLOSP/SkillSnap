namespace SkillSnap.Client.Services
{
  public class UserSessionService
  {
    public int? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? Role { get; private set; }

    public int? CurrentEditingProjectId { get; private set; }
    public string? CurrentEditingProjectTitle { get; private set; }

    public event Action? OnChange;

    public void SetUserSession(int userId, string userName, string role)
    {
      UserId = userId;
      UserName = userName;
      Role = role;
      NotifyStateChanged();
    }

    public void ClearUserSession()
    {
      UserId = null;
      UserName = null;
      Role = null;

      ClearEditingState(false);
      NotifyStateChanged();
    }

    public void SetEditingProject(int projectId, string? projectTitle = null)
    {
      CurrentEditingProjectId = projectId;
      CurrentEditingProjectTitle = projectTitle;
      NotifyStateChanged();
    }

    public void ClearEditingState(bool notify = true)
    {
      CurrentEditingProjectId = null;
      CurrentEditingProjectTitle = null;

      if (notify)
      {
        NotifyStateChanged();
      }
    }

    private void NotifyStateChanged()
    {
      OnChange?.Invoke();
    }
  }
}