using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactUser.Application.Dtos;

public class RoleGetDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
