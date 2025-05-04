Tools and commands to automate common tasks, making it easier for Acumatica developers and administrators to handle customization projects efficiently

## **Acumatica Customization Management CLI Tool**

This CLI tool is designed to automate the deployment of **Acumatica Customization Projects** into Acumatica tenant.

---

### **Who Should Use This Tool?**
- **Developers**: Automate repetitive tasks like building and publishing customization projects.
- **Administrators**: Manage customization projects across tenants with ease.
- **QA Teams**: Quickly reset or deploy customizations for testing purposes.

---

### **Key Features**

1. **Import Customization Projects**:
   - Automatically import `.zip` customization packages into Acumatica.
   - Simplifies the process of adding new customizations to the system.

2. **Publish Customization Projects**:
   - Publish a list of customization projects to the current tenant.
   - Supports replaying database scripts if required.

3. **Unpublish All Customization Projects**:
   - Unpublish all customization projects in the current tenant.
   - Ensures a clean state for testing or deployment.

4. **Retrieve Published Projects**:
   - Fetch and display a list of all currently published customization projects.

---

### **How to Use**
#### **Command-Line Arguments**
The tool accepts various arguments to specify the task to perform, such as:
- `--command`: The action to execute (e.g., `build`, `import`, `publish`, `unpublish`, `getPublished`).
- `--site`: The base URL of the Acumatica instance.
- `--username` and `--password`: Credentials for authentication.
- Additional arguments for file paths, replay options, etc.

---

### **Commands**

**import**

Required parameters:

`--customizationsFolderPath=`, path to the folder containing customization project *.zip files

Example:

```
AcumaticaCustomizationManagementCLI.exe
  --command=import
  --site=https://tenant.acumatica.com/
  --username=username@tenant
  --password=***
  --customizationsFolderPath="C:\PROD_TO_IMPORT"
```

**publish**

Required parameters:

`--projectListPath`, path to the txt file containing list of of projects that need to be published

`--replayDBScripts`, set to `1` to execute all database scripts, `0` to skip already applied scripts (default `0`)

Example:

```
AcumaticaCustomizationManagementCLI.exe`
  --command=publish
  --site=https://tenant.acumatica.com/
  --username=username@tenant
  --password=***
  --projectListPath=ProjectsToPublish.txt
  --replayDBScripts=1
```

**unpublish**

Example:

```
AcumaticaCustomizationManagementCLI.exe`
  --command=unpublish
  --site=https://tenant.acumatica.com/
  --username=username@tenant
  --password=***
```

**getPublished**

Example:

```
AcumaticaCustomizationManagementCLI.exe`
  --command=getPublished
  --site=https://tenant.acumatica.com/
  --username=username@tenant
  --password=***
```

#### **Error Handling**
The tool provides detailed error messages and logs to help identify and resolve issues during execution.
